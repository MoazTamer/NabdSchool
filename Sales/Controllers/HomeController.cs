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
        public async Task<IActionResult> Index()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                // إجمالي عدد الطلاب
                var totalStudents = await _context.TblStudent
                    .CountAsync(s => s.Student_Visible == "yes");

                // عدد الحضور (حاضر + متأخر)
                var presentCount = await _context.TblAttendance
                    .CountAsync(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date == today &&
                                (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));
                
                // عدد المتأخرين
                var lateCount = await _context.TblAttendance
                    .CountAsync(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date == today &&
                                a.Attendance_Status == "متأخر");

                // عدد الغياب
                var absentCount = totalStudents - presentCount;

                // عدد الطالبات المنضبطات (لا غياب - لا تأخير - لا استئذان)
                var disciplinedStudentsCount = await _context.TblStudent
                    .Where(s => s.Student_Visible == "yes")
                    .Where(s =>
                        !_context.TblAttendance.Any(a =>
                            a.Student_ID == s.Student_ID &&
                            a.Attendance_Date.Date == today &&
                            (
                                a.Attendance_Status == "غياب" ||
                                a.Attendance_Status == "متأخر" ||
                                a.Attendance_Status == "استئذان"
                            )
                        )
                    )
                    .CountAsync();

                // الحصول على أفضل الطالبات المنضبطات
                var topStudents = await GetTopStudents(10);
                
                // إحصائيات الشارات
                var badgeStats = await GetBadgeStatistics();

                // اعمل ViewModel
                var model = new DashboardViewModel
                {
                    TotalStudents = totalStudents,
                    Present = presentCount,
                    Late = lateCount,
                    Absent = absentCount,
                    Disciplined = disciplinedStudentsCount,
                    TopStudents = topStudents,
                    BadgeStats = badgeStats
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // في حالة أي خطأ نرجع موديل فارغ
                var model = new DashboardViewModel
                {
                    TotalStudents = 0,
                    Present = 0,
                    Absent = 0,
                    Late = 0,
                    Disciplined = 0,
                    TopStudents = new List<TopStudentBadge>(),
                    BadgeStats = new BadgeStatistics()
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
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
            var today = now.Date;

            bool dailyAbsenceExists = _context.TblAttendance
                .Any(a => a.Attendance_Date == today);

            if (!dailyAbsenceExists)
            {
                InsertDailyAbsence(now, today); 
            }

            var totalStudents = _context.TblStudent.Count(s => s.Student_Visible == "yes");

            var present = _context.TblAttendance
                .Count(a => a.Attendance_Date == today &&
                            (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

            var absent = totalStudents - present;

            var model = new AttendanceViewModel
            {
                TotalStudents = totalStudents,
                Present = present,
                Absent = absent
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationAllStudents()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                // جلب كل الطلاب الظاهرين
                var students = await _context.TblStudent
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => new { s.Student_ID })
                    .ToListAsync();

                // جلب سجلات اليوم
                var todaysRecords = await _context.TblAttendance
                    .Where(a => a.Attendance_Date == today)
                    .Select(a => a.Student_ID)
                    .ToListAsync();
                
                var todaysRecordsSet = todaysRecords.ToHashSet();

                var newAttendances = new List<TblAttendance>();
                var registeredStudents = new List<int>();

                foreach (var student in students)
                {
                    if (!todaysRecordsSet.Contains(student.Student_ID))
                    {
                        newAttendances.Add(new TblAttendance
                        {
                            Student_ID = student.Student_ID,
                            Attendance_Status = "حضور",
                            Attendance_Date = today,
                            Attendance_Time = now.TimeOfDay,
                            Attendance_Visible = "yes",
                            Attendance_AddUserID = "SYSTEM",
                            Attendance_AddDate = now
                        });
                        
                        registeredStudents.Add(student.Student_ID);
                    }
                }

                if (newAttendances.Count > 0)
                {
                    await _context.TblAttendance.AddRangeAsync(newAttendances);
                    await _context.SaveChangesAsync();
                    
                    // تحديث النقاط لكل الطالبات اللي تم تسجيل حضورهم
                    foreach (var studentId in registeredStudents)
                    {
                        try
                        {
                            await UpdateStudentPoints(studentId, today);
                        }
                        catch (Exception ex)
                        {
                            // تجاهل أخطاء النقاط الفردية وكمل
                            Console.WriteLine($"Error updating points for student {studentId}: {ex.Message}");
                        }
                    }
                    
                    return Json(new { 
                        success = true, 
                        message = $"تم تسجيل حضور {newAttendances.Count} طالبة بنجاح" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        message = "جميع الطالبات لديهم سجل حضور اليوم" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "حدث خطأ: " + ex.Message 
                });
            }
        }

        private void InsertDailyAbsence(DateTime now, DateTime today)
        {
            var studentIds = _context.TblStudent
                .Where(s => s.Student_Visible == "yes")
                .Select(s => s.Student_ID)
                .ToList();

            var existingAttendance = _context.TblAttendance
                .Where(a => a.Attendance_Date >= today && a.Attendance_Date < today.AddDays(1))
                .Select(a => a.Student_ID)
                .ToList();

            var newAbsentStudents = studentIds
                .Where(id => !existingAttendance.Contains(id))
                .ToList();

            var attendanceList = newAbsentStudents.Select(id => new TblAttendance
            {
                Student_ID = id,
                Attendance_Date = today,
                Attendance_Status = "غياب",
                Attendance_AddDate = now,
                Attendance_AddUserID = "SYSTEM",
                Attendance_Visible = "yes"
            }).ToList();

            if (attendanceList.Count > 0)
            {
                _context.TblAttendance.AddRange(attendanceList);
                _context.SaveChanges();
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
                        message = "الطالبة غير موجودة أو تم حذفها"
                    });
                }

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                var existingAttendance = await _context.TblAttendance
                    .FirstOrDefaultAsync(a => a.Student_ID == student.Student_ID
                                           && a.Attendance_Date == today
                                           && a.Attendance_Visible == "yes"
                                           && a.Attendance_Status != "غياب");

                if (existingAttendance != null)
                {
                    return await RegisterExcuse(studentCode);
                }

                var attendanceTime = await SchoolSettingsController.GetAttendanceTimeAsync(_context);

                int lateMinutes = 0;
                string status = "حضور";

                if (currentTime > attendanceTime)
                {
                    lateMinutes = (int)(currentTime - attendanceTime).TotalMinutes;
                    status = "متأخر";
                }

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

                // ⭐ تحديث النقاط والشارات بعد تسجيل الحضور
                await UpdateStudentPoints(student.Student_ID, today);

                // الحصول على معلومات النقاط والشارات الجديدة
                var studentPoints = await _context.TblStudentPoints
                    .FirstOrDefaultAsync(sp => sp.Student_ID == student.Student_ID);

                var newBadges = await _context.TblStudentBadges
                    .Where(sb => sb.Student_ID == student.Student_ID && 
                           sb.Badge_Visible == "yes" &&
                           sb.Earned_Date.Date == today)
                    .Join(_context.TblBadgeDefinitions,
                          sb => new { sb.Badge_Type, sb.Badge_Level },
                          bd => new { bd.Badge_Type, bd.Badge_Level },
                          (sb, bd) => new
                          {
                              badge_name = bd.Badge_Name,
                              badge_level = bd.Badge_Level
                          })
                    .ToListAsync();

                string badgeMessage = "";
                if (newBadges.Any())
                {
                    badgeMessage = $" 🏆 مبروك! حصلت على شارة جديدة: {string.Join(", ", newBadges.Select(b => $"{b.badge_name} {b.badge_level}"))}";
                }

                return Json(new
                {
                    isValid = true,
                    message = $"تم تسجيل حضور الطالبة: {student.Student_Name}{badgeMessage}",
                    studentName = student.Student_Name,
                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    attendanceTime = currentTime.ToString(@"hh\:mm"),
                    lateMinutes = lateMinutes,
                    status = status == "متأخر" ? "متأخر" : "حاضر",
                    totalPoints = studentPoints?.Total_Points ?? 0,
                    attendanceStreak = studentPoints?.Attendance_Streak ?? 0,
                    newBadges = newBadges
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
        public async Task<IActionResult> RegisterExcuse(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                    return Json(new { isValid = false, message = "الرجاء إدخال كود الطالبة" });

                studentCode = studentCode.Trim();

                var student = await _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(c => c.Class)
                    .FirstOrDefaultAsync(s => s.Student_Code == studentCode && s.Student_Visible == "yes");

                if (student == null)
                    return Json(new { isValid = false, message = "الطالبة غير موجودة أو تم حذفها" });

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                // هل في استئذان مسجل بالفعل اليوم؟
                var existingExcuse = await _context.TblAttendance
                    .FirstOrDefaultAsync(a =>
                        a.Student_ID == student.Student_ID &&
                        a.Attendance_Date == today &&
                        a.Attendance_Status == "استئذان" &&
                        a.Attendance_Visible == "yes");

                if (existingExcuse != null)
                    return Json(new { isValid = false, message = "تم تسجيل استئذان للطالبة مسبقاً اليوم" });

                // إنشاء سجل استئذان جديد
                var excuse = new TblAttendance
                {
                    Student_ID = student.Student_ID,
                    Attendance_Date = today,
                    Attendance_Time = currentTime,
                    Attendance_LateMinutes = 0,
                    Attendance_Status = "استئذان",
                    Attendance_Visible = "yes",
                    Attendance_AddUserID = _userManager.GetUserId(User),
                    Attendance_AddDate = now
                };

                await _context.TblAttendance.AddAsync(excuse);
                await _context.SaveChangesAsync();

                // ⭐ تحديث النقاط (الاستئذان = 0 نقاط، لكن يقطع سلسلة الحضور)
                await UpdateStudentPoints(student.Student_ID, today);

                return Json(new
                {
                    isValid = true,
                    message = $"تم تسجيل استئذان للطالبة: {student.Student_Name}",
                    studentName = student.Student_Name,
                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    status = "استئذان",
                    attendanceTime = currentTime, 
                });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, message = "خطأ في تسجيل الاستئذان: " + ex.Message });
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

        // ==================== Badge System Methods ====================

        // حساب النقاط للطالبة بناءً على الحضور
        private async Task<int> CalculateStudentPoints(int studentId, DateTime date)
        {
            var attendance = await _context.TblAttendance
                .Where(a => a.Student_ID == studentId &&
                           a.Attendance_Date.Date == date.Date &&
                           a.Attendance_Visible == "yes")
                .FirstOrDefaultAsync();

            if (attendance == null) return 0;

            return attendance.Attendance_Status switch
            {
                "حضور" => 10,      // 10 نقاط للحضور في الوقت
                "متأخر" => 3,      // 3 نقاط فقط للمتأخرة
                "غياب" => -5,      // خصم 5 نقاط للغياب
                "استئذان" => 0,   // لا نقاط للاستئذان
                _ => 0
            };
        }

        // تحديث نقاط الطالبة
        private async Task UpdateStudentPoints(int studentId, DateTime date)
        {
            var points = await CalculateStudentPoints(studentId, date);

            var studentPoints = await _context.TblStudentPoints
                .FirstOrDefaultAsync(sp => sp.Student_ID == studentId);

            if (studentPoints == null)
            {
                studentPoints = new StudentPoints
                {
                    Student_ID = studentId,
                    Total_Points = points,
                    Monthly_Points = points,
                    Attendance_Streak = points > 0 ? 1 : 0,
                    Last_Updated = date
                };
                _context.TblStudentPoints.Add(studentPoints);
            }
            else
            {
                studentPoints.Total_Points += points;
                studentPoints.Monthly_Points += points;

                // تحديث سلسلة الحضور المتتالي
                if (points > 0)
                {
                    var lastDate = studentPoints.Last_Updated.Date;
                    if (date.Date == lastDate.AddDays(1))
                    {
                        studentPoints.Attendance_Streak++;
                    }
                    else if (date.Date > lastDate.AddDays(1))
                    {
                        studentPoints.Attendance_Streak = 1;
                    }
                }
                else if (points < 0 || points == 0) // غياب أو استئذان
                {
                    studentPoints.Attendance_Streak = 0;
                }

                studentPoints.Last_Updated = date;
            }

            await _context.SaveChangesAsync();

            // فحص إذا استحقت الطالبة شارة جديدة
            await CheckAndAwardBadges(studentId);
        }

        // فحص ومنح الشارات
        private async Task CheckAndAwardBadges(int studentId)
        {
            var studentPoints = await _context.TblStudentPoints
                .FirstOrDefaultAsync(sp => sp.Student_ID == studentId);

            if (studentPoints == null) return;

            var existingBadges = await _context.TblStudentBadges
                .Where(sb => sb.Student_ID == studentId && sb.Badge_Visible == "yes")
                .Select(sb => new { sb.Badge_Type, sb.Badge_Level })
                .ToListAsync();

            var badgeDefinitions = await _context.TblBadgeDefinitions
                .Where(bd => bd.Is_Active)
                .OrderBy(bd => bd.Required_Points)
                .ToListAsync();

            foreach (var definition in badgeDefinitions)
            {
                // تحقق إذا الطالبة ما عندهاش الشارة دي
                if (existingBadges.Any(eb => eb.Badge_Type == definition.Badge_Type &&
                                            eb.Badge_Level == definition.Badge_Level))
                    continue;

                bool shouldAward = false;

                // شارات الانضباط (بناءً على إجمالي النقاط)
                if (definition.Badge_Type == "انضباط" &&
                    studentPoints.Total_Points >= definition.Required_Points)
                {
                    shouldAward = true;
                }

                // شارات الحضور المتتالي
                if (definition.Badge_Type == "حضور_متتالي" &&
                    studentPoints.Attendance_Streak >= (definition.Required_Points / 10))
                {
                    shouldAward = true;
                }

                if (shouldAward)
                {
                    var badge = new StudentBadge
                    {
                        Student_ID = studentId,
                        Badge_Type = definition.Badge_Type,
                        Badge_Level = definition.Badge_Level,
                        Points = definition.Required_Points,
                        Earned_Date = DateTime.Now,
                        Badge_Visible = "yes"
                    };

                    _context.TblStudentBadges.Add(badge);
                }
            }

            await _context.SaveChangesAsync();
        }

        // الحصول على أفضل الطالبات
        private async Task<List<TopStudentBadge>> GetTopStudents(int count = 10)
        {
            var topStudents = await _context.TblStudentPoints
                .Include(sp => sp.Student)
                .Where(sp => sp.Student.Student_Visible == "yes")
                .OrderByDescending(sp => sp.Total_Points)
                .Take(count)
                .ToListAsync();

            var result = new List<TopStudentBadge>();

            foreach (var student in topStudents)
            {
                var badges = await _context.TblStudentBadges
                    .Where(sb => sb.Student_ID == student.Student_ID && sb.Badge_Visible == "yes")
                    .Join(_context.TblBadgeDefinitions,
                          sb => new { sb.Badge_Type, sb.Badge_Level },
                          bd => new { bd.Badge_Type, bd.Badge_Level },
                          (sb, bd) => new StudentBadgeInfo
                          {
                              Badge_Name = bd.Badge_Name,
                              Badge_Level = bd.Badge_Level,
                              Badge_Icon = bd.Badge_Icon,
                              Badge_Color = bd.Badge_Color,
                              Points = sb.Points
                          })
                    .ToListAsync();

                var highestBadge = badges.OrderByDescending(b => b.Points).FirstOrDefault();

                result.Add(new TopStudentBadge
                {
                    Student_ID = student.Student_ID,
                    Student_Name = student.Student.Student_Name,
                    Total_Points = student.Total_Points,
                    Attendance_Streak = student.Attendance_Streak,
                    Badges = badges,
                    HighestBadgeLevel = highestBadge?.Badge_Level ?? "لا يوجد",
                    BadgeColor = highestBadge?.Badge_Color ?? "#6c757d"
                });
            }

            return result;
        }

        // إحصائيات الشارات
        private async Task<BadgeStatistics> GetBadgeStatistics()
        {
            var allBadges = await _context.TblStudentBadges
                .Where(sb => sb.Badge_Visible == "yes")
                .ToListAsync();

            return new BadgeStatistics
            {
                TotalBadgesEarned = allBadges.Count,
                DiamondBadges = allBadges.Count(b => b.Badge_Level == "ماسي"),
                GoldBadges = allBadges.Count(b => b.Badge_Level == "ذهبي"),
                SilverBadges = allBadges.Count(b => b.Badge_Level == "فضي"),
                BronzeBadges = allBadges.Count(b => b.Badge_Level == "برونزي")
            };
        }

        // الحصول على تفاصيل شارات طالبة
        [HttpGet]
        public async Task<IActionResult> GetStudentBadges(int studentId)
        {
            try
            {
                var studentPoints = await _context.TblStudentPoints
                    .Include(sp => sp.Student)
                    .FirstOrDefaultAsync(sp => sp.Student_ID == studentId);
                
                var badges = await _context.TblStudentBadges
                    .Where(sb => sb.Student_ID == studentId && sb.Badge_Visible == "yes")
                    .Join(_context.TblBadgeDefinitions,
                          sb => new { sb.Badge_Type, sb.Badge_Level },
                          bd => new { bd.Badge_Type, bd.Badge_Level },
                          (sb, bd) => new
                          {
                              badge_name = bd.Badge_Name,
                              badge_level = bd.Badge_Level,
                              badge_icon = bd.Badge_Icon,
                              badge_color = bd.Badge_Color,
                              points = sb.Points,
                              earned_date = sb.Earned_Date,
                              description = bd.Description
                          })
                    .ToListAsync();
                
                return Json(new
                {
                    success = true,
                    student_name = studentPoints?.Student.Student_Name ?? "",
                    total_points = studentPoints?.Total_Points ?? 0,
                    monthly_points = studentPoints?.Monthly_Points ?? 0,
                    attendance_streak = studentPoints?.Attendance_Streak ?? 0,
                    badges = badges
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // إعادة تعيين النقاط الشهرية (يتم تشغيلها في بداية كل شهر)
        [HttpPost]
        public async Task<IActionResult> ResetMonthlyPoints()
        {
            try
            {
                var allPoints = await _context.TblStudentPoints.ToListAsync();

                foreach (var point in allPoints)
                {
                    point.Monthly_Points = 0;
                }

                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "تم إعادة تعيين النقاط الشهرية بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }
    }
}


//using Dapper;
 //using Microsoft.AspNetCore.Authorization;
 //using Microsoft.AspNetCore.Identity;
 //using Microsoft.AspNetCore.Mvc;
 //using Microsoft.EntityFrameworkCore;
 //using SalesModel.IRepository;
 //using SalesModel.Models;
 //using SalesModel.ViewModels;
 //using SalesRepository.Data;

//namespace Sales.Controllers
//{
//    [Authorize]
//    public class HomeController : Controller
//    {
//        private static TimeZoneInfo Arabian_Standard_Time =
//            TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IAuthorizationService _authorizationService;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SalesDBContext _context;

//        public HomeController(
//            SignInManager<ApplicationUser> signInManager,
//            IUnitOfWork unitOfWork,
//            IAuthorizationService authorizationService,
//            UserManager<ApplicationUser> userManager,
//            SalesDBContext context)
//        {
//            _signInManager = signInManager;
//            _unitOfWork = unitOfWork;
//            _authorizationService = authorizationService;
//            _userManager = userManager;
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult Index()
//        {
//            try
//            {
//                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
//                var today = now.Date;

//                // إجمالي عدد الطلاب
//                var totalStudents = _context.TblStudent
//                    .Count(s => s.Student_Visible == "yes");

//                // عدد الحضور (حاضر + متأخر)
//                var presentCount = _context.TblAttendance
//                    .Count(a => a.Attendance_Visible == "yes" &&
//                                a.Attendance_Date.Date == today &&
//                                (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

//                // عدد المتأخرين
//                var lateCount = _context.TblAttendance
//                    .Count(a => a.Attendance_Visible == "yes" &&
//                                a.Attendance_Date.Date == today &&
//                                a.Attendance_Status == "متأخر");

//                // عدد الغياب
//                var absentCount = totalStudents - presentCount;

//                // عدد الطالبات المنضبطات (لا غياب - لا تأخير - لا استئذان)
//                var disciplinedStudentsCount = _context.TblStudent
//                    .Where(s => s.Student_Visible == "yes")
//                    .Where(s =>
//                        !_context.TblAttendance.Any(a =>
//                            a.Student_ID == s.Student_ID &&
//                            a.Attendance_Date.Date == today &&
//                            (
//                                a.Attendance_Status == "غياب" ||
//                                a.Attendance_Status == "متأخر" ||
//                                a.Attendance_Status == "استئذان"
//                            )
//                        )
//                    )
//                    .Count();


//                // اعمل ViewModel
//                var model = new AttendanceViewModel
//                {
//                    TotalStudents = totalStudents,
//                    Present = presentCount,
//                    Late = lateCount,
//                    Absent = absentCount,
//                    Disciplined = disciplinedStudentsCount
//                };

//                return View(model);
//            }
//            catch (Exception ex)
//            {
//                // في حالة أي خطأ نرجع موديل فارغ
//                var model = new AttendanceViewModel
//                {
//                    TotalStudents = 0,
//                    Present = 0,
//                    Absent = 0
//                };
//                return View(model);
//            }
//        }

//        [HttpGet]
//        public IActionResult Reports()
//        {
//            return View();
//        }

//        [HttpGet]
//        public IActionResult AttendanceRegistration()
//        {
//            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
//            var today = now.Date;

//            bool dailyAbsenceExists = _context.TblAttendance
//                .Any(a => a.Attendance_Date == today);

//            if (!dailyAbsenceExists)
//            {
//                InsertDailyAbsence(now, today); 
//            }

//            var totalStudents = _context.TblStudent.Count(s => s.Student_Visible == "yes");

//            var present = _context.TblAttendance
//                .Count(a => a.Attendance_Date == today &&
//                            (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

//            var absent = totalStudents - present;

//            var model = new AttendanceViewModel
//            {
//                TotalStudents = totalStudents,
//                Present = present,
//                Absent = absent
//            };

//            return View(model);
//        }

//        [HttpPost]
//        public IActionResult RegistrationAllStudents()
//        {
//            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
//            var today = now.Date;

//            // جلب كل الطلاب الظاهرين
//            var students = _context.TblStudent
//                .Where(s => s.Student_Visible == "yes")
//                .Select(s => new { s.Student_ID })
//                .ToList();

//            // جلب سجلات اليوم
//            var todaysRecords = _context.TblAttendance
//                .Where(a => a.Attendance_Date == today)
//                .Select(a => a.Student_ID)
//                .ToHashSet(); 

//            var newAttendances = new List<TblAttendance>();

//            foreach (var student in students)
//            {
//                if (!todaysRecords.Contains(student.Student_ID))
//                {
//                    newAttendances.Add(new TblAttendance
//                    {
//                        Student_ID = student.Student_ID,
//                        Attendance_Status = "حضور",
//                        Attendance_Date = today,
//                        Attendance_Time = now.TimeOfDay,
//                        Attendance_Visible = "yes",
//                        Attendance_AddUserID = "SYSTEM",
//                    });
//                }
//                // لو عنده سجل، لا نغير أي شيء

//            }

//            if (newAttendances.Count > 0)
//                _context.TblAttendance.AddRange(newAttendances);

//            _context.SaveChanges();

//            return Json(new { success = true });
//        }

//        private void InsertDailyAbsence(DateTime now, DateTime today)
//        {
//            var studentIds = _context.TblStudent
//                .Where(s => s.Student_Visible == "yes")
//                .Select(s => s.Student_ID)
//                .ToList();

//            var existingAttendance = _context.TblAttendance
//                .Where(a => a.Attendance_Date >= today && a.Attendance_Date < today.AddDays(1))
//                .Select(a => a.Student_ID)
//                .ToList();

//            var newAbsentStudents = studentIds
//                .Where(id => !existingAttendance.Contains(id))
//                .ToList();

//            var attendanceList = newAbsentStudents.Select(id => new TblAttendance
//            {
//                Student_ID = id,
//                Attendance_Date = today,
//                Attendance_Status = "غياب",
//                Attendance_AddDate = now,
//                Attendance_AddUserID = "SYSTEM",
//                Attendance_Visible = "yes"
//            }).ToList();

//            if (attendanceList.Count > 0)
//            {
//                _context.TblAttendance.AddRange(attendanceList);
//                _context.SaveChanges();
//            }
//        }


//        [HttpPost]
//        public async Task<IActionResult> RegisterAttendance(string studentCode)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(studentCode))
//                {
//                    return Json(new
//                    {
//                        isValid = false,
//                        message = "الرجاء إدخال كود الطالبة"
//                    });
//                }

//                var student = await _context.TblStudent
//                    .Include(s => s.ClassRoom)
//                    .ThenInclude(c => c.Class)
//                    .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim()
//                                           && s.Student_Visible == "yes");

//                if (student == null)
//                {
//                    return Json(new
//                    {
//                        isValid = false,
//                        message = "الطالبة غير موجود أو تم حذفه"
//                    });
//                }

//                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
//                var today = now.Date;
//                var currentTime = now.TimeOfDay;

//                var existingAttendance = await _context.TblAttendance
//                    .FirstOrDefaultAsync(a => a.Student_ID == student.Student_ID
//                                           && a.Attendance_Date == today
//                                           && a.Attendance_Visible == "yes"
//                                           && a.Attendance_Status != "غياب");

//                if (existingAttendance != null)
//                {
//                    return await RegisterExcuse(studentCode);

//                    //await RegisterExcuse(studentCode);
//                    //return Json(new
//                    //{
//                    //    isValid = false,
//                    //    message = $"تم تسجيل حضور الطالبة مسبقاً اليوم الساعة {existingAttendance.Attendance_Time:hh\\:mm}، " +
//                    //              $"{DateTime.Now} وتم الآن تسجيل استئذان للطالبة بناءً على تكرار التسجيل."

//                    //    //message = $"الطالبة {student.Student_Name} سجلت حضورها مسبقاً اليوم الساعة {existingAttendance.Attendance_Time:hh\\:mm} وتم تسجيلها استأذان"
//                    //});
//                }

//                var attendanceTime = await SchoolSettingsController.GetAttendanceTimeAsync(_context);

//                int lateMinutes = 0;
//                string status = "حضور";

//                if (currentTime > attendanceTime)
//                {
//                    lateMinutes = (int)(currentTime - attendanceTime).TotalMinutes;
//                    status = "متأخر";
//                }

//                var attendance = new TblAttendance
//                {
//                    Student_ID = student.Student_ID,
//                    Attendance_Date = today,
//                    Attendance_Time = currentTime,
//                    Attendance_LateMinutes = lateMinutes,
//                    Attendance_Status = status,
//                    Attendance_Visible = "yes",
//                    Attendance_AddUserID = _userManager.GetUserId(User),
//                    Attendance_AddDate = now
//                };

//                await _context.TblAttendance.AddAsync(attendance);
//                var result = await _context.SaveChangesAsync();

//                if (result == 0)
//                {
//                    return Json(new
//                    {
//                        isValid = false,
//                        message = "فشل في حفظ الحضور"
//                    });
//                }

//                return Json(new
//                {
//                    isValid = true,
//                    message = $"تم تسجيل حضور الطالبة: {student.Student_Name}",
//                    studentName = student.Student_Name,
//                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
//                    attendanceTime = currentTime.ToString(@"hh\:mm"),
//                    lateMinutes = lateMinutes,
//                    status = status == "متأخر" ? "متأخر" : "حاضر"
//                });
//            }
//            catch (Exception ex)
//            {
//                return Json(new
//                {
//                    isValid = false,
//                    message = "خطأ في تسجيل الحضور: " + ex.Message
//                });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> RegisterExcuse(string studentCode)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(studentCode))
//                    return Json(new { isValid = false, message = "الرجاء إدخال كود الطالبة" });

//                studentCode = studentCode.Trim();

//                var student = await _context.TblStudent
//                    .Include(s => s.ClassRoom)
//                    .ThenInclude(c => c.Class)
//                    .FirstOrDefaultAsync(s => s.Student_Code == studentCode && s.Student_Visible == "yes");

//                if (student == null)
//                    return Json(new { isValid = false, message = "الطالبة غير موجود أو تم حذفها" });

//                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
//                var today = now.Date;
//                var currentTime = now.TimeOfDay;

//                // هل في استئذان مسجل بالفعل اليوم؟
//                var existingExcuse = await _context.TblAttendance
//                    .FirstOrDefaultAsync(a =>
//                        a.Student_ID == student.Student_ID &&
//                        a.Attendance_Date == today &&
//                        a.Attendance_Status == "استئذان" &&
//                        a.Attendance_Visible == "yes");

//                if (existingExcuse != null)
//                    return Json(new { isValid = false, message = "تم تسجيل استئذان للطالبة مسبقاً اليوم" });

//                // إنشاء سجل استئذان جديد
//                var excuse = new TblAttendance
//                {
//                    Student_ID = student.Student_ID,
//                    Attendance_Date = today,
//                    Attendance_Time = currentTime,
//                    Attendance_LateMinutes = 0,
//                    Attendance_Status = "استئذان",
//                    Attendance_Visible = "yes",
//                    Attendance_AddUserID = _userManager.GetUserId(User),
//                    Attendance_AddDate = now
//                };

//                await _context.TblAttendance.AddAsync(excuse);
//                await _context.SaveChangesAsync();

//                return Json(new
//                {
//                    isValid = true,
//                    message = $"تم تسجيل استئذان للطالبة: {student.Student_Name}",
//                    studentName = student.Student_Name,
//                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
//                    status = "استئذان",
//                    attendanceTime = currentTime, 
//                });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { isValid = false, message = "خطأ في تسجيل الاستئذان: " + ex.Message });
//            }
//        }


//        [HttpPost]
//        public async Task<IActionResult> SignOut()
//        {
//            await _signInManager.SignOutAsync();
//            return Redirect("/Login");
//        }

//        [HttpGet]
//        public IActionResult Error()
//        {
//            return View();
//        }

//        [HttpGet]
//        public IActionResult Authorized()
//        {
//            return View();
//        }




//        public async Task<int> CalculateStudentPoints(int studentId, DateTime date)
//        {
//            var attendance = await _context.TblAttendance
//                .Where(a => a.Student_ID == studentId &&
//                           a.Attendance_Date.Date == date.Date &&
//                           a.Attendance_Visible == "yes")
//                .FirstOrDefaultAsync();

//            if (attendance == null) return 0;

//            return attendance.Attendance_Status switch
//            {
//                "حضور" => 10,      // 10 نقاط للحضور في الوقت
//                "متأخر" => 3,      // 3 نقاط فقط للمتأخرة
//                "غياب" => -5,      // خصم 5 نقاط للغياب
//                "استئذان" => 0,   // لا نقاط للاستئذان
//                _ => 0
//            };
//        }

//        // تحديث نقاط الطالبة
//        public async Task UpdateStudentPoints(int studentId, DateTime date)
//        {
//            var points = await CalculateStudentPoints(studentId, date);

//            var studentPoints = await _context.TblStudentPoints
//                .FirstOrDefaultAsync(sp => sp.Student_ID == studentId);

//            if (studentPoints == null)
//            {
//                studentPoints = new StudentPoints
//                {
//                    Student_ID = studentId,
//                    Total_Points = points,
//                    Monthly_Points = points,
//                    Attendance_Streak = points > 0 ? 1 : 0,
//                    Last_Updated = date
//                };
//                _context.TblStudentPoints.Add(studentPoints);
//            }
//            else
//            {
//                studentPoints.Total_Points += points;
//                studentPoints.Monthly_Points += points;

//                // تحديث سلسلة الحضور المتتالي
//                if (points > 0)
//                {
//                    var lastDate = studentPoints.Last_Updated.Date;
//                    if (date.Date == lastDate.AddDays(1))
//                    {
//                        studentPoints.Attendance_Streak++;
//                    }
//                    else if (date.Date > lastDate.AddDays(1))
//                    {
//                        studentPoints.Attendance_Streak = 1;
//                    }
//                }
//                else if (points < 0) // غياب
//                {
//                    studentPoints.Attendance_Streak = 0;
//                }

//                studentPoints.Last_Updated = date;
//            }

//            await _context.SaveChangesAsync();

//            // فحص إذا استحقت الطالبة شارة جديدة
//            await CheckAndAwardBadges(studentId);
//        }

//        // فحص ومنح الشارات
//        public async Task CheckAndAwardBadges(int studentId)
//        {
//            var studentPoints = await _context.TblStudentPoints
//                .FirstOrDefaultAsync(sp => sp.Student_ID == studentId);

//            if (studentPoints == null) return;

//            var existingBadges = await _context.TblStudentBadges
//                .Where(sb => sb.Student_ID == studentId && sb.Badge_Visible == "yes")
//                .Select(sb => new { sb.Badge_Type, sb.Badge_Level })
//                .ToListAsync();

//            var badgeDefinitions = await _context.TblBadgeDefinitions
//                .Where(bd => bd.Is_Active)
//                .OrderBy(bd => bd.Required_Points)
//                .ToListAsync();

//            foreach (var definition in badgeDefinitions)
//            {
//                // تحقق إذا الطالبة ما عندهاش الشارة دي
//                if (existingBadges.Any(eb => eb.Badge_Type == definition.Badge_Type &&
//                                            eb.Badge_Level == definition.Badge_Level))
//                    continue;

//                bool shouldAward = false;

//                // شارات الانضباط (بناءً على إجمالي النقاط)
//                if (definition.Badge_Type == "انضباط" &&
//                    studentPoints.Total_Points >= definition.Required_Points)
//                {
//                    shouldAward = true;
//                }

//                // شارات الحضور المتتالي
//                if (definition.Badge_Type == "حضور_متتالي" &&
//                    studentPoints.Attendance_Streak >= (definition.Required_Points / 10))
//                {
//                    shouldAward = true;
//                }

//                if (shouldAward)
//                {
//                    var badge = new StudentBadge
//                    {
//                        Student_ID = studentId,
//                        Badge_Type = definition.Badge_Type,
//                        Badge_Level = definition.Badge_Level,
//                        Points = definition.Required_Points,
//                        Earned_Date = DateTime.Now,
//                        Badge_Visible = "yes"
//                    };

//                    _context.TblStudentBadges.Add(badge);
//                }
//            }

//            await _context.SaveChangesAsync();
//        }

//        // الحصول على أفضل الطالبات
//        public async Task<List<TopStudentBadge>> GetTopStudents(int count = 10)
//        {
//            var topStudents = await _context.TblStudentPoints
//                .Include(sp => sp.Student)
//                .Where(sp => sp.Student.Student_Visible == "yes")
//                .OrderByDescending(sp => sp.Total_Points)
//                .Take(count)
//                .ToListAsync();

//            var result = new List<TopStudentBadge>();

//            foreach (var student in topStudents)
//            {
//                var badges = await _context.TblStudentBadges
//                    .Where(sb => sb.Student_ID == student.Student_ID && sb.Badge_Visible == "yes")
//                    .Join(_context.TblBadgeDefinitions,
//                          sb => new { sb.Badge_Type, sb.Badge_Level },
//                          bd => new { bd.Badge_Type, bd.Badge_Level },
//                          (sb, bd) => new StudentBadgeInfo
//                          {
//                              Badge_Name = bd.Badge_Name,
//                              Badge_Level = bd.Badge_Level,
//                              Badge_Icon = bd.Badge_Icon,
//                              Badge_Color = bd.Badge_Color,
//                              Points = sb.Points
//                          })
//                    .ToListAsync();

//                var highestBadge = badges.OrderByDescending(b => b.Points).FirstOrDefault();

//                result.Add(new TopStudentBadge
//                {
//                    Student_ID = student.Student_ID,
//                    Student_Name = student.Student.Student_Name,
//                    Total_Points = student.Total_Points,
//                    Attendance_Streak = student.Attendance_Streak,
//                    Badges = badges,
//                    HighestBadgeLevel = highestBadge?.Badge_Level ?? "لا يوجد",
//                    BadgeColor = highestBadge?.Badge_Color ?? "#6c757d"
//                });
//            }

//            return result;
//        }

//        // إحصائيات الشارات
//        public async Task<BadgeStatistics> GetBadgeStatistics()
//        {
//            var allBadges = await _context.TblStudentBadges
//                .Where(sb => sb.Badge_Visible == "yes")
//                .ToListAsync();

//            return new BadgeStatistics
//            {
//                TotalBadgesEarned = allBadges.Count,
//                DiamondBadges = allBadges.Count(b => b.Badge_Level == "ماسي"),
//                GoldBadges = allBadges.Count(b => b.Badge_Level == "ذهبي"),
//                SilverBadges = allBadges.Count(b => b.Badge_Level == "فضي"),
//                BronzeBadges = allBadges.Count(b => b.Badge_Level == "برونزي")
//            };
//        }

//        // إعادة تعيين النقاط الشهرية (يتم تشغيلها في بداية كل شهر)
//        public async Task ResetMonthlyPoints()
//        {
//            var allPoints = await _context.TblStudentPoints.ToListAsync();

//            foreach (var point in allPoints)
//            {
//                point.Monthly_Points = 0;
//            }

//            await _context.SaveChangesAsync();
//        }
//    }
//}