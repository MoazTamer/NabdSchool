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
    public class SchoolSettingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SalesDBContext _context;
        private const string AttendanceTimeKey = "AttendanceTime";
        private const string AcademicYearKey = "AcademicYear";
        private const string SemesterKey = "Semester";
        string Title = "إعدادات المدرسة";

        public SchoolSettingsController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SalesDBContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }

        [Authorize (Policy = "SchoolSettings_View")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var attendanceSetting = await _context.TblSchoolSettings
                    .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                    .FirstOrDefaultAsync();

                var academicYearSetting = await _context.TblSchoolSettings
                    .Where(obj => obj.Setting_Key == AcademicYearKey && obj.Setting_Visible == "yes")
                    .FirstOrDefaultAsync();

                var semesterSetting = await _context.TblSchoolSettings
                    .Where(obj => obj.Setting_Key == SemesterKey && obj.Setting_Visible == "yes")
                    .FirstOrDefaultAsync();

                var model = new ModelSchoolSettings
                {
                    Setting_ID = attendanceSetting?.Setting_ID ?? 0,
                    AttendanceTime = attendanceSetting?.Setting_Value ?? "07:30",
                    AcademicYear = academicYearSetting?.Setting_Value ?? "1447",
                    Semester = semesterSetting?.Setting_Value ?? "الأول"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "خطأ في تحميل البيانات: " + ex.Message;
                return View(new ModelSchoolSettings
                {
                    AttendanceTime = "07:30",
                    AcademicYear = "1447",
                    Semester = "الأول"
                });
            }
        }


        [Authorize(Policy = "SchoolSettings_Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(ModelSchoolSettings model)
        {
            try
            {
                // التحقق من البيانات
                if (string.IsNullOrEmpty(model.AttendanceTime))
                {
                    return Json(new { isValid = false, title = Title, message = "الرجاء إدخال موعد الحضور" });
                }

                if (string.IsNullOrEmpty(model.AcademicYear))
                {
                    return Json(new { isValid = false, title = Title, message = "الرجاء إدخال العام الدراسي" });
                }

                if (string.IsNullOrEmpty(model.Semester))
                {
                    return Json(new { isValid = false, title = Title, message = "الرجاء اختيار الفصل الدراسي" });
                }

                if (!TimeSpan.TryParse(model.AttendanceTime, out TimeSpan time))
                {
                    return Json(new { isValid = false, title = Title, message = "صيغة الوقت غير صحيحة" });
                }

                var userId = _userManager.GetUserId(User);
                var currentDate = DateTime.Now;

                // حفظ موعد الحضور
                await SaveOrUpdateSetting(AttendanceTimeKey, model.AttendanceTime, "موعد الحضور اليومي للمدرسة", userId, currentDate);

                // حفظ العام الدراسي
                await SaveOrUpdateSetting(AcademicYearKey, model.AcademicYear, "العام الدراسي الحالي", userId, currentDate);

                // حفظ الفصل الدراسي
                await SaveOrUpdateSetting(SemesterKey, model.Semester, "الفصل الدراسي الحالي", userId, currentDate);

                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم حفظ الإعدادات بنجاح" });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ?
                    ex.InnerException.Message : ex.Message;

                return Json(new
                {
                    isValid = false,
                    title = Title,
                    message = $"خطأ: {innerMessage}"
                });
            }
        }

        private async Task SaveOrUpdateSetting(string key, string value, string description, string userId, DateTime currentDate)
        {
            var setting = await _context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == key && obj.Setting_Visible == "yes")
                .FirstOrDefaultAsync();

            if (setting == null)
            {
                var newSetting = new TblSchoolSettings
                {
                    Setting_Key = key,
                    Setting_Value = value,
                    Setting_Description = description,
                    Setting_Visible = "yes",
                    Setting_AddUserID = userId,
                    Setting_AddDate = currentDate
                };

                await _context.TblSchoolSettings.AddAsync(newSetting);
            }
            else
            {
                setting.Setting_Value = value;
                setting.Setting_EditUserID = userId;
                setting.Setting_EditDate = currentDate;

                _context.TblSchoolSettings.Update(setting);
            }
        }

        // Methods للحصول على القيم
        public static async Task<TimeSpan> GetAttendanceTimeAsync(SalesDBContext context)
        {
            var setting = await context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                .FirstOrDefaultAsync();

            if (setting != null && TimeSpan.TryParse(setting.Setting_Value, out TimeSpan time))
            {
                return time;
            }

            return new TimeSpan(7, 30, 0);
        }

        public static TimeSpan GetAttendanceTime(SalesDBContext context)
        {
            var setting = context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                .FirstOrDefault();

            if (setting != null && TimeSpan.TryParse(setting.Setting_Value, out TimeSpan time))
            {
                return time;
            }

            return new TimeSpan(7, 30, 0);
        }

        public static async Task<string> GetAcademicYearAsync(SalesDBContext context)
        {
            var setting = await context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == AcademicYearKey && obj.Setting_Visible == "yes")
                .FirstOrDefaultAsync();

            return setting?.Setting_Value ?? "1447";
        }

        public static string GetAcademicYear(SalesDBContext context)
        {
            var setting = context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == AcademicYearKey && obj.Setting_Visible == "yes")
                .FirstOrDefault();

            return setting?.Setting_Value ?? "1447";
        }

        public static async Task<string> GetSemesterAsync(SalesDBContext context)
        {
            var setting = await context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == SemesterKey && obj.Setting_Visible == "yes")
                .FirstOrDefaultAsync();

            return setting?.Setting_Value ?? "الأول";
        }

        public static string GetSemester(SalesDBContext context)
        {
            var setting = context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == SemesterKey && obj.Setting_Visible == "yes")
                .FirstOrDefault();

            return setting?.Setting_Value ?? "الأول";
        }
    }
}