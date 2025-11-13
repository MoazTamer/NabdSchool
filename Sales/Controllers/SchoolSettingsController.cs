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
        private readonly SalesDBContext _context; // إضافة الـ context مباشرة
        private const string AttendanceTimeKey = "AttendanceTime";
        string Title = "إعدادات المدرسة";

        public SchoolSettingsController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SalesDBContext context) // إضافة الـ context
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // جلب موعد الحضور من قاعدة البيانات مباشرة
                var attendanceSetting = await _context.TblSchoolSettings
                    .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                    .FirstOrDefaultAsync();

                var model = new ModelSchoolSettings
                {
                    Setting_ID = attendanceSetting?.Setting_ID ?? 0,
                    Setting_Key = AttendanceTimeKey,
                    AttendanceTime = attendanceSetting?.Setting_Value ?? "07:30",
                    Setting_Description = attendanceSetting?.Setting_Description ?? "موعد الحضور اليومي للمدرسة"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "خطأ في تحميل البيانات: " + ex.Message;
                return View(new ModelSchoolSettings { AttendanceTime = "07:30" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendanceTime(ModelSchoolSettings model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.AttendanceTime))
                {
                    return Json(new { isValid = false, title = Title, message = "الرجاء إدخال موعد الحضور" });
                }

                // التحقق من صيغة الوقت
                if (!TimeSpan.TryParse(model.AttendanceTime, out TimeSpan time))
                {
                    return Json(new { isValid = false, title = Title, message = "صيغة الوقت غير صحيحة" });
                }

                // البحث عن الإعداد الموجود
                var attendanceSetting = await _context.TblSchoolSettings
                    .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                    .FirstOrDefaultAsync();

                if (attendanceSetting == null)
                {
                    // إنشاء إعداد جديد
                    var newSetting = new TblSchoolSettings
                    {
                        Setting_Key = AttendanceTimeKey,
                        Setting_Value = model.AttendanceTime,
                        Setting_Description = "موعد الحضور اليومي للمدرسة",
                        Setting_Visible = "yes",
                        Setting_AddUserID = _userManager.GetUserId(User),
                        Setting_AddDate = DateTime.Now
                    };

                    await _context.TblSchoolSettings.AddAsync(newSetting);
                }
                else
                {
                    // تحديث الإعداد الموجود
                    attendanceSetting.Setting_Value = model.AttendanceTime;
                    attendanceSetting.Setting_EditUserID = _userManager.GetUserId(User);
                    attendanceSetting.Setting_EditDate = DateTime.Now;

                    _context.TblSchoolSettings.Update(attendanceSetting);
                }

                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم حفظ موعد الحضور بنجاح" });
            }
            catch (Exception ex)
            {
                // إظهار تفاصيل الخطأ الكاملة
                var innerMessage = ex.InnerException != null ?
                    ex.InnerException.Message : ex.Message;
                var stackTrace = ex.StackTrace;

                return Json(new
                {
                    isValid = false,
                    title = Title,
                    message = $"خطأ: {innerMessage}",
                    details = stackTrace
                });
            }
        }

        // Helper method للحصول على موعد الحضور (يمكن استخدامه في أماكن أخرى)
        public static async Task<TimeSpan> GetAttendanceTimeAsync(SalesDBContext context)
        {
            var setting = await context.TblSchoolSettings
                .Where(obj => obj.Setting_Key == AttendanceTimeKey && obj.Setting_Visible == "yes")
                .FirstOrDefaultAsync();

            if (setting != null && TimeSpan.TryParse(setting.Setting_Value, out TimeSpan time))
            {
                return time;
            }

            // القيمة الافتراضية إذا لم يتم العثور على إعداد
            return new TimeSpan(7, 30, 0); // 7:30 صباحاً
        }

        // نسخة Sync من الـ Helper method
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
    }
}