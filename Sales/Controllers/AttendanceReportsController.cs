using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesModel.IRepository;
using SalesRepository.Data;
using System.Globalization;

namespace Sales.Controllers
{
    [Authorize]
    public class AttendanceReportsController : Controller
    {
            private readonly IUnitOfWork _unitOfWork;
            private static readonly TimeZoneInfo Arabian_Standard_Time =
                TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

            public AttendanceReportsController(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            [HttpGet]
            public IActionResult Index()
            {
                return View();
            }

        [HttpGet]
        public async Task<IActionResult> GetWeeklyReport(DateTime? startDate)
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var weekStart = startDate ?? now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Saturday);
                var weekEnd = weekStart.AddDays(6);

                var arabicCulture = new CultureInfo("ar-SA");

                var totalStudents = await _unitOfWork.TblStudent
                    .CountAsync(s => s.Student_Visible == "yes");

                var attendanceByDate = _unitOfWork.TblAttendance
                    .GetAll(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date >= weekStart &&
                                a.Attendance_Date.Date <= weekEnd)
                    .GroupBy(a => a.Attendance_Date.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Present = g.Count(a => a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"),
                            Late = g.Count(a => a.Attendance_Status == "متأخر"),
                            Excused = g.Count(a => a.Attendance_Status == "استئذان")
                        }
                    );

                var report = new List<object>();

                for (int i = 0; i < 7; i++)
                {
                    var currentDate = weekStart.AddDays(i);

                    var dayStats = attendanceByDate.ContainsKey(currentDate)
                        ? attendanceByDate[currentDate]
                        : new { Present = 0, Late = 0, Excused = 0 };

                    var presentCount = dayStats.Present;
                    var lateCount = dayStats.Late;
                    var excusedCount = dayStats.Excused;
                    var absentCount = totalStudents - presentCount;
                    var onTimeCount = presentCount - lateCount;

                    report.Add(new
                    {
                        date = currentDate.ToString("yyyy-MM-dd"),
                        dayName = currentDate.ToString("dddd", arabicCulture),
                        dayNumber = currentDate.Day,
                        totalStudents = totalStudents,
                        present = presentCount,
                        onTime = onTimeCount,
                        late = lateCount,
                        absent = absentCount,
                        excused = excusedCount,
                        attendancePercentage = totalStudents > 0
                            ? Math.Round((presentCount * 100.0) / totalStudents, 1)
                            : 0
                    });
                }

                return Json(new
                {
                    success = true,
                    data = report,
                    weekStart = weekStart.ToString("yyyy-MM-dd"),
                    weekEnd = weekEnd.ToString("yyyy-MM-dd"),
                    totalStudents = totalStudents
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyReport(int? year, int? month)
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var targetYear = year ?? now.Year;
                var targetMonth = month ?? now.Month;

                var monthStart = new DateTime(targetYear, targetMonth, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var daysInMonth = DateTime.DaysInMonth(targetYear, targetMonth);

                var arabicCulture = new CultureInfo("ar-EG");

                var totalStudents = await _unitOfWork.TblStudent
                    .CountAsync(s => s.Student_Visible == "yes");

                var attendanceByDate = _unitOfWork.TblAttendance
                    .GetAll(a => a.Attendance_Visible == "yes" &&
                                a.Attendance_Date.Date >= monthStart &&
                                a.Attendance_Date.Date <= monthEnd)
                    .GroupBy(a => a.Attendance_Date.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Present = g.Count(a => a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"),
                            Late = g.Count(a => a.Attendance_Status == "متأخر"),
                            Excused = g.Count(a => a.Attendance_Status == "استئذان")
                        }
                    );

                var report = new List<object>();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(targetYear, targetMonth, day);

                    var dayStats = attendanceByDate.ContainsKey(currentDate)
                        ? attendanceByDate[currentDate]
                        : new { Present = 0, Late = 0, Excused = 0 };

                    var presentCount = dayStats.Present;
                    var lateCount = dayStats.Late;
                    var excusedCount = dayStats.Excused;
                    var absentCount = totalStudents - presentCount;
                    var onTimeCount = presentCount - lateCount;

                    report.Add(new
                    {
                        date = currentDate.ToString("yyyy-MM-dd"),
                        dayName = currentDate.ToString("dddd", arabicCulture),
                        dayNumber = day,
                        totalStudents = totalStudents,
                        present = presentCount,
                        onTime = onTimeCount,
                        late = lateCount,
                        absent = absentCount,
                        excused = excusedCount,
                        attendancePercentage = totalStudents > 0
                            ? Math.Round((presentCount * 100.0) / totalStudents, 1)
                            : 0
                    });
                }

                return Json(new
                {
                    success = true,
                    data = report,
                    monthName = monthStart.ToString("MMMM yyyy", arabicCulture),
                    monthStart = monthStart.ToString("yyyy-MM-dd"),
                    monthEnd = monthEnd.ToString("yyyy-MM-dd"),
                    totalStudents = totalStudents
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReportSummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                startDate = startDate.Date;
                endDate = endDate.Date.AddDays(1).AddSeconds(-1);

                var totalStudents = await _unitOfWork.TblStudent
                    .CountAsync(s => s.Student_Visible == "yes");

                var attendanceData = _unitOfWork.TblAttendance
                    .GetAll(
                        a => a.Attendance_Visible == "yes" &&
                                     a.Attendance_Date >= startDate &&
                                     a.Attendance_Date <= endDate
                    )
                    .GroupBy(a => a.Attendance_Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                var totalPresent = attendanceData
                    .Where(x => x.Status == "حضور" || x.Status == "متأخر")
                    .Sum(x => x.Count);

                var totalLate = attendanceData
                    .Where(x => x.Status == "متأخر")
                    .Sum(x => x.Count);

                var totalExcused = attendanceData
                    .Where(x => x.Status == "استئذان")
                    .Sum(x => x.Count);

                var daysCount = (endDate.Date - startDate.Date).Days + 1;

                var totalAbsent = (totalStudents * daysCount) - totalPresent;

                return Json(new
                {
                    success = true,
                    summary = new
                    {
                        totalStudents = totalStudents,
                        daysCount = daysCount,
                        totalPresent = totalPresent,
                        totalLate = totalLate,
                        totalAbsent = totalAbsent,
                        totalExcused = totalExcused,
                        averageAttendance = totalStudents > 0 && daysCount > 0
                            ? Math.Round((totalPresent * 100.0) / (totalStudents * daysCount), 1)
                            : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}