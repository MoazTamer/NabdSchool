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

        //[Authorize (Policy = "SchoolSettings_View")]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var keys = new[] { AttendanceTimeKey, AcademicYearKey, SemesterKey };

                var settings = _unitOfWork.TblSchoolSettings
                    .GetAll(s => keys.Contains(s.Setting_Key) && s.Setting_Visible == "yes")
                    .ToList();

                var attendance = settings.FirstOrDefault(s => s.Setting_Key == AttendanceTimeKey);
                var academicYear = settings.FirstOrDefault(s => s.Setting_Key == AcademicYearKey);
                var semester = settings.FirstOrDefault(s => s.Setting_Key == SemesterKey);

                var model = new ModelSchoolSettings
                {
                    Setting_ID = attendance?.Setting_ID ?? 0,
                    AttendanceTime = attendance?.Setting_Value ?? "07:30",
                    AcademicYear = academicYear?.Setting_Value ?? "1447",
                    Semester = semester?.Setting_Value ?? "الأول"
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


        //[Authorize(Policy = "SchoolSettings_Edit")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveSettings(ModelSchoolSettings model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AttendanceTime))
                    return Json(new { isValid = false, title = Title, message = "الرجاء إدخال موعد الحضور" });

                if (string.IsNullOrEmpty(model.AcademicYear))
                    return Json(new { isValid = false, title = Title, message = "الرجاء إدخال العام الدراسي" });

                if (string.IsNullOrEmpty(model.Semester))
                    return Json(new { isValid = false, title = Title, message = "الرجاء اختيار الفصل الدراسي" });

                if (!TimeSpan.TryParse(model.AttendanceTime, out _))
                    return Json(new { isValid = false, title = Title, message = "صيغة الوقت غير صحيحة" });

                var userId = _userManager.GetUserId(User);
                var now = DateTime.Now;

                var keys = new[] { AttendanceTimeKey, AcademicYearKey, SemesterKey };

                var settings = _unitOfWork.TblSchoolSettings
                    .GetAll(s => keys.Contains(s.Setting_Key) && s.Setting_Visible == "yes")
                    .ToList();

                await SaveOrUpdateSetting(settings, AttendanceTimeKey, model.AttendanceTime,
                    "موعد الحضور اليومي", userId, now);

                await SaveOrUpdateSetting(settings, AcademicYearKey, model.AcademicYear,
                    "العام الدراسي", userId, now);

                await SaveOrUpdateSetting(settings, SemesterKey, model.Semester,
                    "الفصل الدراسي", userId, now);

                return Json(new { isValid = true, title = Title, message = "تم حفظ الإعدادات بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isValid = false,
                    title = Title,
                    message = "خطأ: " + (ex.InnerException?.Message ?? ex.Message)
                });
            }
        }

        private async Task SaveOrUpdateSetting(
             List<TblSchoolSettings> settingsList,
             string key,
             string value,
             string description,
             string userId,
             DateTime now)
        {
            var existing = settingsList.FirstOrDefault(s => s.Setting_Key == key);

            if (existing == null)
            {
                var newSetting = new TblSchoolSettings
                {
                    Setting_Key = key,
                    Setting_Value = value,
                    Setting_Description = description,
                    Setting_Visible = "yes",
                    Setting_AddUserID = userId,
                    Setting_AddDate = now
                };

                _unitOfWork.TblSchoolSettings.Add(newSetting);

                settingsList.Add(newSetting);
            }
            else
            {
                await _unitOfWork.TblSchoolSettings.UpdateAll(
                    s => s.Setting_Key == key,
                    updateExpression: setters => setters
                        .SetProperty(s => s.Setting_Value, value)
                        .SetProperty(s => s.Setting_EditUserID, userId)
                        .SetProperty(s => s.Setting_EditDate, now)
                );
            }
        }


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

        public static TimeSpan GetAttendanceTime(IUnitOfWork unit)
        {
            var setting = unit.TblSchoolSettings
                .GetFirstOrDefault(s => s.Setting_Key == AttendanceTimeKey && s.Setting_Visible == "yes");

            if (setting != null && TimeSpan.TryParse(setting.Setting_Value, out TimeSpan time))
                return time;

            return new TimeSpan(7, 30, 0);
        }

        public static string GetAcademicYear(IUnitOfWork unit)
        {
            var setting = unit.TblSchoolSettings
                .GetFirstOrDefault(s => s.Setting_Key == AcademicYearKey && s.Setting_Visible == "yes");

            return setting?.Setting_Value ?? "1447";
        }

        public static string GetSemester(IUnitOfWork unit)
        {
            var setting = unit.TblSchoolSettings
                .GetFirstOrDefault(s => s.Setting_Key == SemesterKey && s.Setting_Visible == "yes");

            return setting?.Setting_Value ?? "الأول";
        }

    }
}