using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NabdSchool.Core.Interfaces;
using NabdSchool.Web.ViewModels;
using System.Text.Json;

namespace NabdSchool.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ArchiveController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArchiveController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Archive
        public async Task<IActionResult> Index(string searchTable = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var auditLogs = (await _unitOfWork.AuditLogs.GetAllAsync()).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTable))
            {
                auditLogs = auditLogs.Where(a => a.TableName.Contains(searchTable));
            }

            if (fromDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.DateTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                auditLogs = auditLogs.Where(a => a.DateTime <= toDate.Value.AddDays(1));
            }

            var logs = auditLogs
                .OrderByDescending(a => a.DateTime)
                .Take(500) // Limit to last 500 records
                .ToList();

            ViewBag.SearchTable = searchTable;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(logs);
        }

        // GET: Archive/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            var model = new AuditLogDetailsViewModel
            {
                AuditLog = log,
                OldValuesFormatted = FormatJson(log.OldValues),
                NewValuesFormatted = FormatJson(log.NewValues)
            };

            return View(model);
        }

        // GET: Archive/Students
        public async Task<IActionResult> Students()
        {
            var allStudents = await _unitOfWork.Students.GetAllAsync();
            var deletedStudents = allStudents.Where(s => !s.IsVisible).ToList();

            return View(deletedStudents);
        }

        // POST: Archive/RestoreStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStudent(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
            {
                TempData["Error"] = "الطالب غير موجود";
                return RedirectToAction(nameof(Students));
            }

            student.IsVisible = true;
            student.DeletedBy = null;
            student.DeletedDate = null;
            student.ModifiedBy = User.Identity.Name;
            student.ModifiedDate = DateTime.Now;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"تم استرجاع الطالب {student.FullName} بنجاح";
            return RedirectToAction(nameof(Students));
        }

        // GET: Archive/Statistics
        public async Task<IActionResult> Statistics()
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();

            var stats = new ArchiveStatisticsViewModel
            {
                TotalLogs = logs.Count(),
                ActionsByType = logs.GroupBy(l => l.Action)
                    .Select(g => new ActionStatistic
                    {
                        ActionType = g.Key,
                        Count = g.Count()
                    }).ToList(),
                ActionsByTable = logs.GroupBy(l => l.TableName)
                    .Select(g => new TableStatistic
                    {
                        TableName = g.Key,
                        Count = g.Count()
                    }).ToList(),
                TopUsers = logs.GroupBy(l => l.UserName)
                    .Select(g => new UserActivityStatistic
                    {
                        UserName = g.Key,
                        ActionCount = g.Count(),
                        LastActivity = g.Max(x => x.DateTime)
                    })
                    .OrderByDescending(u => u.ActionCount)
                    .Take(10)
                    .ToList(),
                ActivityByDate = logs.GroupBy(l => l.DateTime.Date)
                    .Select(g => new DateActivityStatistic
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(d => d.Date)
                    .Take(30)
                    .ToList()
            };

            return View(stats);
        }

        // POST: Archive/ClearOldLogs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearOldLogs(int months = 6)
        {
            var cutoffDate = DateTime.Now.AddMonths(-months);
            var oldLogs = await _unitOfWork.AuditLogs.FindAsync(l => l.DateTime < cutoffDate);

            if (oldLogs.Any())
            {
                await _unitOfWork.AuditLogs.DeleteRangeAsync(oldLogs);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"تم حذف {oldLogs.Count()} سجل قديم (أقدم من {months} أشهر)";
            }
            else
            {
                TempData["Info"] = "لا توجد سجلات قديمة للحذف";
            }

            return RedirectToAction(nameof(Index));
        }

        private string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return "لا توجد بيانات";

            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                return json;
            }
        }
    }
}
