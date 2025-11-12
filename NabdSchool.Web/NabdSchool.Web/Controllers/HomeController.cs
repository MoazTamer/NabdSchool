using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NabdSchool.Core.Interfaces;
using NabdSchool.Web.ViewModels;

namespace NabdSchool.Web.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var allStudents = await _unitOfWork.Students.GetAllAsync();
            var activeStudents = allStudents.Where(s => s.IsVisible).ToList();
            var deletedStudents = allStudents.Where(s => !s.IsVisible).ToList();

            // Group by Grade
            var studentsByGrade = activeStudents
                .GroupBy(s => s.GradeId)
                .Select(g => new GradeStatistic
                {
                    GradeId = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.GradeId)
                .ToList();


            // Recent activities from Audit Log
            var recentActivities = (await _unitOfWork.AuditLogs.GetAllAsync())
                .OrderByDescending(a => a.DateTime)
                .Take(10)
                .ToList();

            var dashboard = new DashboardViewModel
            {
                TotalStudents = activeStudents.Count,
                DeletedStudents = deletedStudents.Count,
                TotalGrades = activeStudents.Select(s => s.Grade).Distinct().Count(),
                TotalClasses = activeStudents.Select(s => new { s.GradeId, s.ClassId }).Distinct().Count(),
                StudentsByGrade = studentsByGrade,
                RecentActivities = recentActivities
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
