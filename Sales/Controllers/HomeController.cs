using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;
using SalesRepository.Data;

namespace Sales.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static TimeZoneInfo Arabian_Standard_Time =
            TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SalesDBContext _context;

        public HomeController(
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager,
            SalesDBContext context)
        {
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                // إجمالي عدد الطلاب
                var totalStudents = _context.TblStudent
                    .Count(s => s.Student_Visible == "yes");

                // عدد الحضور (حاضر + متأخر)
                var presentCount = _context.TblAttendance
                    .Count(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date == today &&
                                (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));
                
                // عدد المتأخرين
                var lateCount = _context.TblAttendance
                    .Count(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date == today &&
                                a.Attendance_Status == "متأخر");

                // عدد الغياب
                var absentCount = _context.TblAttendance
                    .Count(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date == today &&
                                a.Attendance_Status == "غياب");

                // اعمل ViewModel
                var model = new AttendanceViewModel
                {
                    TotalStudents = totalStudents,
                    Present = presentCount,
                    Late = lateCount,
                    Absent = absentCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // في حالة أي خطأ نرجع موديل فارغ
                var model = new AttendanceViewModel
                {
                    TotalStudents = 0,
                    Present = 0,
                    Absent = 0
                };
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Reports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AttendanceRegistration()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                // IDs كل الطلاب
                var studentIds = _context.TblStudent
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .ToList();

                // الحضور الموجود اليوم
                var existingAttendance = _context.TblAttendance
                    .Where(a => a.Attendance_Date >= today && a.Attendance_Date < today.AddDays(1))
                    .ToList();

                // الطلاب اللي ما عندهمش حضور
                var newAbsentStudents = studentIds
                    .Where(id => !existingAttendance.Any(a => a.Student_ID == id))
                    .ToList();

                // تسجيل الغياب للطلاب الجدد
                var attendanceList = newAbsentStudents.Select(id => new TblAttendance
                {
                    Student_ID = id,
                    Attendance_Date = today,
                    Attendance_Status = "غياب",
                    Attendance_AddDate = now,
                    Attendance_AddUserID = _userManager.GetUserId(User),
                    Attendance_Visible = "yes"
                }).ToList();

                _context.TblAttendance.AddRange(attendanceList);
                _unitOfWork.Complete();

                // احسب الأرقام
                int totalStudents = studentIds.Count;
                int present = existingAttendance.Count(a => a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر");
                int absent = totalStudents - present;

                // ابعت موديل مع الفيو
                var model = new AttendanceViewModel
                {
                    TotalStudents = totalStudents,
                    Present = present,
                    Absent = absent
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // ممكن تبعت ViewModel فيها 0 أو رسالة خطأ
                var model = new AttendanceViewModel { TotalStudents = 0, Present = 0, Absent = 0 };
                ViewBag.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAttendance(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                {
                    return Json(new
                    {
                        isValid = false,
                        message = "الرجاء إدخال كود الطالبة"
                    });
                }

                // البحث عن الطالب
                var student = await _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(c => c.Class)
                    .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim()
                                           && s.Student_Visible == "yes");

                if (student == null)
                {
                    return Json(new
                    {
                        isValid = false,
                        message = "الطالبة غير موجود أو تم حذفه"
                    });
                }

                // الحصول على التاريخ والوقت الحالي
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                // التحقق من التسجيل المسبق اليوم
                var existingAttendance = await _context.TblAttendance
                    .FirstOrDefaultAsync(a => a.Student_ID == student.Student_ID
                                           && a.Attendance_Date == today
                                           && a.Attendance_Visible == "yes"
                                           && a.Attendance_Status != "غياب");

                if (existingAttendance != null)
                {
                    return Json(new
                    {
                        isValid = false,
                        message = $"الطالبة {student.Student_Name} سجلت حضورها مسبقاً اليوم الساعة {existingAttendance.Attendance_Time:hh\\:mm}"
                    });
                }

                // الحصول على موعد الحضور من الإعدادات
                var attendanceTime = await SchoolSettingsController.GetAttendanceTimeAsync(_context);

                // حساب دقائق التأخير
                int lateMinutes = 0;
                string status = "حضور";

                if (currentTime > attendanceTime)
                {
                    lateMinutes = (int)(currentTime - attendanceTime).TotalMinutes;
                    status = "متأخر";
                }

                // تسجيل الحضور
                var attendance = new TblAttendance
                {
                    Student_ID = student.Student_ID,
                    Attendance_Date = today,
                    Attendance_Time = currentTime,
                    Attendance_LateMinutes = lateMinutes,
                    Attendance_Status = status,
                    Attendance_Visible = "yes",
                    Attendance_AddUserID = _userManager.GetUserId(User),
                    Attendance_AddDate = now
                };

                await _context.TblAttendance.AddAsync(attendance);
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return Json(new
                    {
                        isValid = false,
                        message = "فشل في حفظ الحضور"
                    });
                }

                return Json(new
                {
                    isValid = true,
                    message = $"تم تسجيل حضور الطالبة: {student.Student_Name}",
                    studentName = student.Student_Name,
                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    attendanceTime = currentTime.ToString(@"hh\:mm"),
                    lateMinutes = lateMinutes,
                    status = status == "متأخر" ? "متأخر" : "حاضر"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isValid = false,
                    message = "خطأ في تسجيل الحضور: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/Login");
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Authorized()
        {
            return View();
        }
    }
}