using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesRepository.Data;
using System.Globalization;

namespace Sales.Controllers
{
    [Authorize]
    public class AttendanceReportsController : Controller
    {
            private readonly SalesDBContext _context;
            private static readonly TimeZoneInfo Arabian_Standard_Time =
                TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

            public AttendanceReportsController(SalesDBContext context)
            {
                _context = context;
            }

            // صفحة التقارير الرئيسية
            [HttpGet]
            public IActionResult Index()
            {
                return View();
            }

            // الحصول على تقرير أسبوعي
            [HttpGet]
            public async Task<IActionResult> GetWeeklyReport(DateTime? startDate)
            {
                try
                {
                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                    var weekStart = startDate ?? now.Date.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Saturday);
                    var weekEnd = weekStart.AddDays(6);

                    var report = new List<object>();
                    var arabicCulture = new CultureInfo("ar-EG");

                    // إجمالي عدد الطلاب
                    var totalStudents = await _context.TblStudent
                        .CountAsync(s => s.Student_Visible == "yes");

                    // حلقة على أيام الأسبوع
                    for (int i = 0; i < 7; i++)
                    {
                        var currentDate = weekStart.AddDays(i);

                        // عدد الحضور (حضور + متأخر)
                        var presentCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

                        // عدد المتأخرين
                        var lateCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        a.Attendance_Status == "متأخر");

                        // عدد المستئذنين (اللي راحوا بدري)
                        var excusedCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        a.Attendance_Status == "استئذان");

                        // عدد الغياب = إجمالي الطلاب - الحضور
                        var absentCount = totalStudents - presentCount;

                        // عدد الحضور الفعلي (بدون المتأخرين)
                        var onTimeCount = presentCount - lateCount;

                        report.Add(new
                        {
                            date = currentDate.ToString("yyyy-MM-dd"),
                            dayName = currentDate.ToString("dddd", arabicCulture),
                            dayNumber = currentDate.Day,
                            totalStudents = totalStudents,
                            present = presentCount, // حضور + متأخر
                            onTime = onTimeCount,   // حضور في الوقت
                            late = lateCount,       // متأخرين
                            absent = absentCount,   // غياب
                            excused = excusedCount, // مستئذنين (راحوا بدري)
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

            // الحصول على تقرير شهري
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

                    var report = new List<object>();
                    var arabicCulture = new CultureInfo("ar-EG");

                    // إجمالي عدد الطلاب
                    var totalStudents = await _context.TblStudent
                        .CountAsync(s => s.Student_Visible == "yes");

                    // حلقة على أيام الشهر
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        var currentDate = new DateTime(targetYear, targetMonth, day);

                        // عدد الحضور (حضور + متأخر)
                        var presentCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

                        // عدد المتأخرين
                        var lateCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        a.Attendance_Status == "متأخر");

                        // عدد المستئذنين
                        var excusedCount = await _context.TblAttendance
                            .CountAsync(a => a.Attendance_Visible == "yes" &&
                                        a.Attendance_Date.Date == currentDate &&
                                        a.Attendance_Status == "استئذان");

                        // عدد الغياب
                        var absentCount = totalStudents - presentCount;

                        // عدد الحضور الفعلي
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

            // الحصول على ملخص التقرير
            [HttpGet]
            public async Task<IActionResult> GetReportSummary(DateTime startDate, DateTime endDate)
            {
                try
                {
                    var totalStudents = await _context.TblStudent
                        .CountAsync(s => s.Student_Visible == "yes");

                    var attendanceData = await _context.TblAttendance
                        .Where(a => a.Attendance_Visible == "yes" &&
                                   a.Attendance_Date.Date >= startDate.Date &&
                                   a.Attendance_Date.Date <= endDate.Date)
                        .GroupBy(a => a.Attendance_Status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

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