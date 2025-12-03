using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;
using SalesModel.ViewModels.Reports;
using SalesRepository.Data;
using SalesRepository.Repository;

namespace Sales.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ReportPdfService _pdfService;

        private string _academicYear;
        private string _semester;


        public ReportsController(ReportPdfService pdfService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _academicYear = SchoolSettingsController.GetAcademicYear(_unitOfWork);
            _semester = SchoolSettingsController.GetSemester(_unitOfWork);
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult GetClasses()
        {
            try
            {
                var classes = _unitOfWork.TblClass
                    .GetAll(c => c.Class_Visible == "yes")
                    .Select(c => new { id = c.Class_ID, name = c.Class_Name })
                    .ToList();

                return Json(new { success = true, data = classes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetClassRooms(int classId)
        {
            try
            {
                var classRooms = _unitOfWork.TblClassRoom
                    .GetAll(cr => cr.Class_ID == classId && cr.ClassRoom_Visible == "yes")
                    .OrderBy(cr => cr.ClassRoom_Name)
                    .Select(cr => new { id = cr.ClassRoom_ID, name = cr.ClassRoom_Name })
                    .ToList();

                return Json(new { success = true, data = classRooms });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // تقرير الغياب اليومي
        [HttpGet]
        public IActionResult DailyAbsence()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDailyAbsenceReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;

                var query = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes",
                    includeProperties: new[] { "Class", "Students" }

                );

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = query.ToList();

                var todayAttendance = _unitOfWork.TblAttendance
                    .GetAll(a =>
                        a.Attendance_Date.Date == reportDate.Date &&
                        a.Attendance_Visible == "yes"
                    )
                    .Select(a => new { a.Student_ID, a.Attendance_Status })
                    .ToList();

                if (!todayAttendance.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = new DailyAbsenceReportViewModel
                        {
                            ReportDate = reportDate,
                            ClassesAbsence = new List<ClassAbsenceViewModel>(),
                            TotalAbsentStudents = 0,
                            TotalClasses = 0
                        }
                    });
                }

                var presentTodaySet = todayAttendance
                    .Where(x => x.Attendance_Status == "حضور" || x.Attendance_Status == "متأخر" || x.Attendance_Status == "استئذان")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                var relevantStudentIds = classRooms
                    .SelectMany(cr => cr.Students)
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                var minDate = reportDate.AddDays(-30);

                var last30 = _unitOfWork.TblAttendance
                    .GetAll(a =>
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID)
                    )
                    .Select(a => new
                    {
                        a.Student_ID,
                        Date = a.Attendance_Date.Date,
                        Status = a.Attendance_Status,
                        Time = a.Attendance_Time
                    })
                    .ToList();

                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .GroupBy(x => x.Date)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg.OrderByDescending(x => x.Time).First().Status
                            )
                    );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students
                        .Where(s => s.Student_Visible == "yes")
                        .ToList();

                    var studentIds = activeStudents
                        .Select(s => s.Student_ID)
                        .ToList();

                    var absentIds = studentIds
                        .Where(id => !presentTodaySet.Contains(id))
                        .ToList();

                    if (!absentIds.Any())
                        continue;

                    var absentList = new List<AbsentStudentViewModel>();

                    foreach (var sid in absentIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0;
                        int currentStreak = 0;

                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null &&
                                dateStatusMap.TryGetValue(day, out var statusOnDay) &&
                                statusOnDay == "غياب")
                            {
                                currentStreak++;
                                maxConsecutive = Math.Max(maxConsecutive, currentStreak);
                            }
                            else
                            {
                                currentStreak = 0;
                            }
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        absentList.Add(new AbsentStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = maxConsecutive,
                            Notes = maxConsecutive >= 3 ? "تحذير: غياب متكرر" : ""
                        });
                    }

                    classesAbsence.Add(new ClassAbsenceViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = activeStudents.Count,
                        AbsentStudents = absentIds.Count,
                        AbsencePercentage =
                            activeStudents.Count > 0
                            ? Math.Round((decimal)absentIds.Count / activeStudents.Count * 100, 2)
                            : 0,
                        AbsentStudentsList = absentList.OrderBy(s => s.StudentName).ToList()
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence
                        .OrderBy(c => c.ClassName)
                        .ThenBy(c => c.ClassRoomName)
                        .ToList(),
                    TotalAbsentStudents = classesAbsence.Sum(c => c.AbsentStudents),
                    TotalClasses = classesAbsence.Count
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintDailyAbsencePdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes"
                        && (!classId.HasValue || cr.Class_ID == classId.Value)
                        && (!classRoomId.HasValue || cr.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "Class", "Students" }
                );

                classRooms = classRooms.ToList();

                var relevantStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                if (!relevantStudentIds.Any())
                {
                    var emptyView = new DailyAbsenceReportViewModel
                    {
                        ReportDate = reportDate,
                        ClassesAbsence = new List<ClassAbsenceViewModel>(),
                        TotalAbsentStudents = 0,
                        TotalClasses = 0
                    };

                    return File(
                        _pdfService.GenerateDailyAbsenceReport(emptyView),
                        "application/pdf",
                        $"تقرير_الغياب_اليومي_{date:yyyy-MM-dd}.pdf"
                    );
                }

                var todayRecords = _unitOfWork.TblAttendance.GetAll(
                    a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID)
                );

                var presentTodaySet = todayRecords
                    .Where(x => x.Attendance_Status == "حضور" || x.Attendance_Status == "متأخر" || x.Attendance_Status == "استئذان")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                var minDate = reportDate.AddDays(-30);

                var last30 = _unitOfWork.TblAttendance.GetAll(
                    a =>
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID)
                );

                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .GroupBy(x => x.Attendance_Date.Date)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg
                                    .OrderByDescending(x => x.Attendance_Time)
                                    .First()
                                    .Attendance_Status
                            )
                    );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students?
                            .Where(s => s.Student_Visible == "yes")
                            .ToList()
                        ?? new List<TblStudent>();

                    var studentIds = activeStudents.Select(s => s.Student_ID).ToList();

                    var absentIds = studentIds
                        .Where(id => !presentTodaySet.Contains(id))
                        .ToList();

                    if (!absentIds.Any())
                        continue;

                    var absentList = new List<AbsentStudentViewModel>();

                    foreach (var sid in absentIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxStreak = 0, streak = 0;

                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null &&
                                dateStatusMap.TryGetValue(day, out var s))
                            {
                                if (s == "غياب")
                                {
                                    streak++;
                                    maxStreak = Math.Max(maxStreak, streak);
                                }
                                else streak = 0;
                            }
                            else streak = 0;
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        absentList.Add(new AbsentStudentViewModel
                        {
                            StudentId = st.Student_ID,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = maxStreak,
                            Notes = maxStreak >= 3 ? "تحذير: غياب متكرر" : ""
                        });
                    }

                    classesAbsence.Add(new ClassAbsenceViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = activeStudents.Count,
                        AbsentStudents = absentIds.Count,
                        AbsencePercentage = activeStudents.Count > 0
                            ? Math.Round((decimal)absentIds.Count / activeStudents.Count * 100, 2)
                            : 0,
                        AbsentStudentsList = absentList.OrderBy(s => s.StudentName).ToList()
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence
                        .OrderBy(c => c.ClassName)
                        .ThenBy(c => c.ClassRoomName)
                        .ToList(),
                    TotalAbsentStudents = classesAbsence.Sum(c => c.AbsentStudents),
                    TotalClasses = classesAbsence.Count
                };

                var pdfBytes = _pdfService.GenerateDailyAbsenceReport(viewModel);
                return File(pdfBytes, "application/pdf", $"تقرير_الغياب_اليومي_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // تقرير الخروج المبكر
        [HttpGet]
        public IActionResult DailyEarlyExit()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDailyEarlyExitReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes"
                          && (!classId.HasValue || cr.Class_ID == classId.Value)
                          && (!classRoomId.HasValue || cr.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                if (!allStudentIds.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = new DailyEarlyExitReportViewModel
                        {
                            ReportDate = reportDate,
                            ClassesReport = new List<ClassEarlyExitViewModel>()
                        }
                    });
                }

                var todayRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date == reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                         && a.Attendance_Status == "استئذان"
                ).ToList();

                var earlyExitSet = todayRecords.Select(r => r.Student_ID).ToHashSet();

                var classesReport = new List<ClassEarlyExitViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var students = classRoom.Students?
                        .Where(s => s.Student_Visible == "yes")
                        .ToList() ?? new List<TblStudent>();

                    var studentIds = students.Select(s => s.Student_ID).ToList();
                    var earlyExitIds = studentIds.Where(id => earlyExitSet.Contains(id)).ToList();

                    if (!earlyExitIds.Any()) continue;

                    var exitList = earlyExitIds.Select(sid =>
                    {
                        var student = students.First(s => s.Student_ID == sid);
                        var time = todayRecords
                            .Where(r => r.Student_ID == sid)
                            .OrderByDescending(r => r.Attendance_Time)
                            .FirstOrDefault()?.Attendance_Time;

                        return new EarlyExitStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = student.Student_Name,
                            StudentCode = student.Student_Code,
                            StudentPhone = student.Student_Phone,
                            ExitTime = time?.ToString(@"hh\:mm") ?? ""
                        };
                    }).OrderBy(x => x.StudentName).ToList();

                    classesReport.Add(new ClassEarlyExitViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = students.Count,
                        EarlyExitStudents = exitList.Count,
                        EarlyExitStudentsList = exitList
                    });
                }

                return Json(new
                {
                    success = true,
                    data = new DailyEarlyExitReportViewModel
                    {
                        ReportDate = reportDate,
                        ClassesReport = classesReport
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintDailyEarlyExitPdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes"
                          && (!classId.HasValue || cr.Class_ID == classId.Value)
                          && (!classRoomId.HasValue || cr.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                if (!allStudentIds.Any())
                {
                    var emptyView = new DailyEarlyExitReportViewModel
                    {
                        ReportDate = reportDate,
                        ClassesReport = new List<ClassEarlyExitViewModel>()
                    };
                    return File(_pdfService.GenerateDailyEarlyExitReport(emptyView),
                                "application/pdf",
                                $"تقرير_الخروج_المبكر_{date:yyyy-MM-dd}.pdf");
                }

                var todayRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date == reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                         && a.Attendance_Status == "استئذان"
                ).ToList();

                var earlyExitSet = todayRecords.Select(r => r.Student_ID).ToHashSet();

                var classesReport = new List<ClassEarlyExitViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var students = classRoom.Students?
                        .Where(s => s.Student_Visible == "yes")
                        .ToList() ?? new List<TblStudent>();

                    var earlyExitIds = students
                        .Where(s => earlyExitSet.Contains(s.Student_ID))
                        .Select(s => s.Student_ID)
                        .ToList();

                    if (!earlyExitIds.Any()) continue;

                    var exitList = earlyExitIds.Select(id =>
                    {
                        var st = students.First(s => s.Student_ID == id);
                        var time = todayRecords
                            .Where(r => r.Student_ID == id)
                            .OrderByDescending(r => r.Attendance_Time)
                            .FirstOrDefault()?.Attendance_Time;

                        return new EarlyExitStudentViewModel
                        {
                            StudentId = id,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ExitTime = time?.ToString(@"hh\:mm") ?? ""
                        };
                    }).OrderBy(x => x.StudentName).ToList();

                    classesReport.Add(new ClassEarlyExitViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = students.Count,
                        EarlyExitStudents = exitList.Count,
                        EarlyExitStudentsList = exitList
                    });
                }

                var viewModel = new DailyEarlyExitReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesReport = classesReport
                };

                var pdfBytes = _pdfService.GenerateDailyEarlyExitReport(viewModel);

                return File(pdfBytes, "application/pdf", $"تقرير_الخروج_المبكر_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        // تقرير التأخر اليومي
        public IActionResult DailyLate()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDailyLateReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes"
                                  && (!classId.HasValue || cr.Class_ID == classId.Value)
                                  && (!classRoomId.HasValue || cr.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                if (!allStudentIds.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = new DailyAbsenceReportViewModel
                        {
                            ReportDate = reportDate,
                            ClassesAbsence = new List<ClassAbsenceViewModel>(),
                            TotalAbsentStudents = 0,
                            TotalClasses = 0
                        }
                    });
                }

                var todayAttendance = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date == reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                ).Select(a => new
                {
                    a.Student_ID,
                    a.Attendance_Status,
                    a.Attendance_Time
                }).ToList();

                var lateToday = todayAttendance
                    .Where(a => a.Attendance_Status == "متأخر")
                    .Select(a => new { a.Student_ID, a.Attendance_Time })
                    .ToList();

                var lateTodayIds = lateToday.Select(x => x.Student_ID).ToHashSet();

                var minDate = reportDate.AddDays(-30);
                var last30Attendance = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date >= minDate
                         && a.Attendance_Date.Date <= reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                ).Select(a => new
                {
                    a.Student_ID,
                    Date = a.Attendance_Date.Date,
                    Status = a.Attendance_Status,
                    Time = a.Attendance_Time
                }).ToList();

                var studentDateStatus = last30Attendance
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(x => x.Date)
                              .ToDictionary(
                                  gg => gg.Key,
                                  gg => gg.OrderByDescending(x => x.Time).First().Status
                              )
                    );

                var studentAttendanceTime = lateToday.ToDictionary(x => x.Student_ID, x => x.Attendance_Time);

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students?.Where(s => s.Student_Visible == "yes").ToList()
                                         ?? new List<TblStudent>();

                    var classLateIds = activeStudents
                        .Where(s => lateTodayIds.Contains(s.Student_ID))
                        .Select(s => s.Student_ID)
                        .ToList();

                    if (!classLateIds.Any()) continue;

                    var lateList = new List<AbsentStudentViewModel>();

                    foreach (var sid in classLateIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0, currentStreak = 0;

                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var status))
                            {
                                if (string.Equals(status, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    if (currentStreak > maxConsecutive)
                                        maxConsecutive = currentStreak;
                                }
                                else currentStreak = 0;
                            }
                            else currentStreak = 0;
                        }

                        var student = activeStudents.First(s => s.Student_ID == sid);

                        studentAttendanceTime.TryGetValue(sid, out var attendanceTime);

                        lateList.Add(new AbsentStudentViewModel
                        {
                            StudentId = student.Student_ID,
                            StudentName = student.Student_Name,
                            StudentCode = student.Student_Code,
                            StudentPhone = student.Student_Phone,
                            AttendanceTime = attendanceTime,
                            ConsecutiveAbsenceDays = maxConsecutive,
                            Notes = maxConsecutive >= 3 ? "تحذير: تأخر متكرر" : ""
                        });
                    }

                    classesAbsence.Add(new ClassAbsenceViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = activeStudents.Count,
                        AbsentStudents = classLateIds.Count,
                        AbsencePercentage = activeStudents.Count > 0
                            ? Math.Round((decimal)classLateIds.Count / activeStudents.Count * 100, 2)
                            : 0,
                        AbsentStudentsList = lateList.OrderBy(s => s.StudentName).ToList()
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence.OrderBy(c => c.ClassName)
                                                   .ThenBy(c => c.ClassRoomName)
                                                   .ToList(),
                    TotalAbsentStudents = classesAbsence.Sum(c => c.AbsentStudents),
                    TotalClasses = classesAbsence.Count
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintDailyLatePdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes"
                                  && (!classId.HasValue || cr.Class_ID == classId.Value)
                                  && (!classRoomId.HasValue || cr.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                if (!allStudentIds.Any())
                {
                    var emptyView = new DailyAbsenceReportViewModel
                    {
                        ReportDate = reportDate,
                        ClassesAbsence = new List<ClassAbsenceViewModel>(),
                        TotalAbsentStudents = 0,
                        TotalClasses = 0
                    };
                    var emptyPdf = _pdfService.GenerateDailyLateReport(emptyView);
                    return File(emptyPdf, "application/pdf", $"تقرير_التأخر_اليومي_{date:yyyy-MM-dd}.pdf");
                }

                var todayRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date == reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                ).Select(a => new { a.Student_ID, a.Attendance_Status, a.Attendance_Time }).ToList();

                var lateToday = todayRecords
                    .Where(a => a.Attendance_Status == "متأخر")
                    .Select(a => new { a.Student_ID, a.Attendance_Time })
                    .ToList();

                var lateTodayIds = lateToday.Select(x => x.Student_ID).ToHashSet();
                var studentAttendanceTime = lateToday.ToDictionary(x => x.Student_ID, x => x.Attendance_Time);

                var minDate = reportDate.AddDays(-30);
                var last30 = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Date.Date >= minDate
                         && a.Attendance_Date.Date <= reportDate
                         && a.Attendance_Visible == "yes"
                         && allStudentIds.Contains(a.Student_ID)
                ).Select(a => new
                {
                    a.Student_ID,
                    Date = a.Attendance_Date.Date,
                    Status = a.Attendance_Status,
                    Time = a.Attendance_Time
                }).ToList();

                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(x => x.Date)
                              .ToDictionary(
                                  gg => gg.Key,
                                  gg => gg.OrderByDescending(x => x.Time).First().Status
                              )
                    );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students?.Where(s => s.Student_Visible == "yes").ToList() ?? new List<TblStudent>();

                    var classLateIds = activeStudents
                        .Where(s => lateTodayIds.Contains(s.Student_ID))
                        .Select(s => s.Student_ID)
                        .ToList();

                    if (!classLateIds.Any()) continue;

                    var lateList = classLateIds.Select(sid =>
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);
                        int maxConsecutive = 0, currentStreak = 0;

                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;
                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var status))
                            {
                                if (string.Equals(status, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    maxConsecutive = Math.Max(maxConsecutive, currentStreak);
                                }
                                else currentStreak = 0;
                            }
                            else currentStreak = 0;
                        }

                        var student = activeStudents.First(s => s.Student_ID == sid);
                        studentAttendanceTime.TryGetValue(sid, out var attendanceTime);

                        return new AbsentStudentViewModel
                        {
                            StudentId = student.Student_ID,
                            StudentName = student.Student_Name,
                            StudentCode = student.Student_Code,
                            StudentPhone = student.Student_Phone,
                            AttendanceTime = attendanceTime,
                            ConsecutiveAbsenceDays = maxConsecutive,
                            Notes = maxConsecutive >= 3 ? "تحذير: تأخر متكرر" : ""
                        };
                    }).OrderBy(s => s.StudentName).ToList();

                    var latePercentage = activeStudents.Count > 0
                        ? Math.Round((decimal)classLateIds.Count / activeStudents.Count * 100, 2)
                        : 0;

                    classesAbsence.Add(new ClassAbsenceViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name ?? "غير محدد",
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = activeStudents.Count,
                        AbsentStudents = classLateIds.Count,
                        AbsencePercentage = latePercentage,
                        AbsentStudentsList = lateList
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence.OrderBy(c => c.ClassName).ThenBy(c => c.ClassRoomName).ToList(),
                    TotalAbsentStudents = classesAbsence.Sum(c => c.AbsentStudents),
                    TotalClasses = classesAbsence.Count
                };

                var pdfBytes = _pdfService.GenerateDailyLateReport(viewModel);
                return File(pdfBytes, "application/pdf", $"تقرير_التأخر_اليومي_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // تقرير طالب معين
        public IActionResult StudentReport(string studentIdentifier, DateTime? fromDate, DateTime? date)
        {
            ViewBag.StudentIdentifier = studentIdentifier;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Date = date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }


        [HttpGet]
        public IActionResult GetStudentAttendanceReport(string studentIdentifier, DateTime? date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);

                studentIdentifier = studentIdentifier.Trim();

                var student = _unitOfWork.TblStudent.GetAll(
                    s => (s.Student_Code == studentIdentifier || s.Student_Name.Contains(studentIdentifier))
                                 && s.Student_Visible == "yes",
                    includeProperties: new[] { "ClassRoom.Class" }
                ).FirstOrDefault();

                if (student == null)
                    return Json(new { success = false, message = "الطالبة غير موجود" });

                var records = _unitOfWork.TblAttendance.GetAll(
                    a => a.Student_ID == student.Student_ID
                                && a.Attendance_Visible == "yes"
                                && a.Attendance_Date.Date >= minDate
                                && a.Attendance_Date.Date <= reportDate
                ).Select(a => new
                {
                    Date = a.Attendance_Date.Date,
                    a.Attendance_Status,
                    a.Attendance_Time
                }).ToList();

                var result = new List<StudentDayStatusViewModel>();
                for (var day = reportDate; day >= minDate; day = day.AddDays(-1))
                {
                    var record = records
                        .Where(r => r.Date == day)
                        .OrderByDescending(r => r.Attendance_Time)
                        .FirstOrDefault();

                    if (record != null && !string.IsNullOrEmpty(record.Attendance_Status))
                    {
                        result.Add(new StudentDayStatusViewModel
                        {
                            Date = day,
                            Status = record.Attendance_Status,
                            Time = record.Attendance_Time.ToString(@"hh\:mm"),
                            Notes = ""
                        });
                    }
                }

                int present = result.Count(r => r.Status == "حضور" || r.Status == "متأخر" || r.Status == "استئذان");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                int maxConsecutiveLate = 0, currentLateCount = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر") currentLateCount++;
                    else currentLateCount = 0;
                    if (currentLateCount > maxConsecutiveLate) maxConsecutiveLate = currentLateCount;
                }

                int maxConsecutiveAbsent = 0, currentAbsentCount = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب") currentAbsentCount++;
                    else currentAbsentCount = 0;
                    if (currentAbsentCount > maxConsecutiveAbsent) maxConsecutiveAbsent = currentAbsentCount;
                }

                var viewModel = new StudentAttendanceReportViewModel
                {
                    StudentId = student.Student_ID,
                    StudentName = student.Student_Name,
                    StudentCode = student.Student_Code,
                    ReportDate = reportDate,
                    Days = result.OrderByDescending(r => r.Date).ToList(),
                    TotalPresent = present,
                    TotalLate = late,
                    TotalAbsent = absent,
                    ConsecutiveLate = maxConsecutiveLate,
                    ConsecutiveAbsent = maxConsecutiveAbsent
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintStudentAttendancePdf(string studentIdentifier, DateTime date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date.Date;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);

                studentIdentifier = studentIdentifier.Trim();

                var student = _unitOfWork.TblStudent.GetAll(
                    s => (s.Student_Code == studentIdentifier || s.Student_Name.Contains(studentIdentifier))
                                 && s.Student_Visible == "yes",
                    includeProperties: new[] { "ClassRoom.Class" }
                ).FirstOrDefault();

                if (student == null)
                    return Json(new { success = false, message = "الطالبة غير موجودة" });

                var records = _unitOfWork.TblAttendance.GetAll(
                    a => a.Student_ID == student.Student_ID
                                && a.Attendance_Visible == "yes"
                                && a.Attendance_Date.Date >= minDate
                                && a.Attendance_Date.Date <= reportDate
                ).Select(a => new
                {
                    Date = a.Attendance_Date.Date,
                    a.Attendance_Status,
                    a.Attendance_Time
                }).ToList();

                var result = new List<StudentDayStatusViewModel>();
                for (var day = reportDate; day >= minDate; day = day.AddDays(-1))
                {
                    var record = records
                        .Where(r => r.Date == day)
                        .OrderByDescending(r => r.Attendance_Time)
                        .FirstOrDefault();

                    if (record != null && !string.IsNullOrEmpty(record.Attendance_Status))
                    {
                        result.Add(new StudentDayStatusViewModel
                        {
                            Date = day,
                            Status = record.Attendance_Status,
                            Time = record.Attendance_Time.ToString(@"hh\:mm"),
                            Notes = ""
                        });
                    }
                }

                int present = result.Count(r => r.Status == "حضور" || r.Status == "متأخر" || r.Status == "استئذان");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                int maxConsecutiveLate = 0, currentLateCount = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر") currentLateCount++;
                    else currentLateCount = 0;
                    if (currentLateCount > maxConsecutiveLate) maxConsecutiveLate = currentLateCount;
                }

                int maxConsecutiveAbsent = 0, currentAbsentCount = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب") currentAbsentCount++;
                    else currentAbsentCount = 0;
                    if (currentAbsentCount > maxConsecutiveAbsent) maxConsecutiveAbsent = currentAbsentCount;
                }

                var viewModel = new StudentAttendanceReportViewModel
                {
                    StudentId = student.Student_ID,
                    StudentName = student.Student_Name,
                    StudentCode = student.Student_Code,
                    ClassName = student.ClassRoom?.Class?.Class_Name ?? "غير محدد",
                    ClassRoomName = student.ClassRoom?.ClassRoom_Name ?? "غير محدد",
                    ReportDate = reportDate,
                    FromDate = minDate,
                    Days = result.OrderByDescending(r => r.Date).ToList(),
                    TotalPresent = present,
                    TotalLate = late,
                    TotalAbsent = absent,
                    ConsecutiveLate = maxConsecutiveLate,
                    ConsecutiveAbsent = maxConsecutiveAbsent
                };

                var pdfBytes = _pdfService.GenerateStudentAttendanceReport(viewModel);
                return File(pdfBytes, "application/pdf", $"تقرير_حضور_{student.Student_Name}_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        // تقرير أكثر الطلاب غياب
        [HttpGet]
        public IActionResult MostAbsentStudents()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetMostAbsentStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes"
                                 && (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value)
                                 && (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes"
                                && studentIds.Contains(a.Student_ID)
                                && a.Attendance_Date.Date >= startDate
                                && a.Attendance_Date.Date <= endDate
                                && a.Attendance_Status == "غياب"
                ).ToList();

                var result = students
                    .Select(s =>
                    {
                        var absentDays = attendanceRecords.Count(a => a.Student_ID == s.Student_ID);
                        return new MostAbsentStudentViewModel
                        {
                            StudentId = s.Student_ID,
                            StudentName = s.Student_Name,
                            StudentCode = s.Student_Code,
                            ClassName = s.ClassRoom.Class.Class_Name,
                            ClassRoomName = s.ClassRoom.ClassRoom_Name,
                            AbsentDays = absentDays,
                            TotalDays = (endDate - startDate).Days + 1
                        };
                    })
                    .Where(s => s.AbsentDays > 0)
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintMostAbsentStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes"
                                 && (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value)
                                 && (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes"
                                && studentIds.Contains(a.Student_ID)
                                && a.Attendance_Date.Date >= startDate
                                && a.Attendance_Date.Date <= endDate
                                && a.Attendance_Status == "غياب"
                ).ToList();

                var result = students
                    .Select(s =>
                    {
                        var absentDays = attendanceRecords.Count(a => a.Student_ID == s.Student_ID);
                        return new MostAbsentStudentViewModel
                        {
                            StudentId = s.Student_ID,
                            StudentName = s.Student_Name,
                            StudentCode = s.Student_Code,
                            ClassName = s.ClassRoom.Class.Class_Name,
                            ClassRoomName = s.ClassRoom.ClassRoom_Name,
                            AbsentDays = absentDays,
                            TotalDays = (endDate - startDate).Days + 1
                        };
                    })
                    .Where(s => s.AbsentDays > 0)
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToList();

                var viewModel = new MostAbsentStudentsReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    ClassRoomId = classRoomId,
                    TopCount = top,
                    Students = result,
                    TotalStudents = result.Count,
                    TotalAbsentDays = result.Sum(s => s.AbsentDays)
                };

                var pdfBytes = _pdfService.GenerateMostAbsentStudentsReport(viewModel);
                return File(pdfBytes, "application/pdf", $"أكثر_الطلاب_غياب_{fromDate:yyyy-MM-dd}_إلى_{toDate:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // تقرير أكثر الطلاب تأخر
        [HttpGet]
        public IActionResult MostLateStudents()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetMostLateStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes"
                                 && (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value)
                                 && (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes"
                                && studentIds.Contains(a.Student_ID)
                                && a.Attendance_Date.Date >= startDate
                                && a.Attendance_Date.Date <= endDate
                                && a.Attendance_Status == "متأخر"
                ).ToList();

                var result = students
                    .Select(s =>
                    {
                        var lateDays = attendanceRecords.Count(a => a.Student_ID == s.Student_ID);
                        return new MostAbsentStudentViewModel
                        {
                            StudentId = s.Student_ID,
                            StudentName = s.Student_Name,
                            StudentCode = s.Student_Code,
                            ClassName = s.ClassRoom.Class.Class_Name,
                            ClassRoomName = s.ClassRoom.ClassRoom_Name,
                            AbsentDays = lateDays,
                            TotalDays = (endDate - startDate).Days + 1
                        };
                    })
                    .Where(s => s.AbsentDays > 0)
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintMostLateStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes"
                                 && (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value)
                                 && (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes"
                                && studentIds.Contains(a.Student_ID)
                                && a.Attendance_Date.Date >= startDate
                                && a.Attendance_Date.Date <= endDate
                                && a.Attendance_Status == "متأخر"
                ).ToList();

                var result = students
                    .Select(s =>
                    {
                        var lateDays = attendanceRecords.Count(a => a.Student_ID == s.Student_ID);
                        return new MostAbsentStudentViewModel
                        {
                            StudentId = s.Student_ID,
                            StudentName = s.Student_Name,
                            StudentCode = s.Student_Code,
                            ClassName = s.ClassRoom.Class.Class_Name,
                            ClassRoomName = s.ClassRoom.ClassRoom_Name,
                            AbsentDays = lateDays,
                            TotalDays = (endDate - startDate).Days + 1
                        };
                    })
                    .Where(s => s.AbsentDays > 0)
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToList();

                var viewModel = new MostAbsentStudentsReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    ClassRoomId = classRoomId,
                    TopCount = top,
                    Students = result,
                    TotalStudents = result.Count,
                    TotalAbsentDays = result.Sum(s => s.AbsentDays),
                    ReportType = "تأخر"
                };

                var pdfBytes = _pdfService.GenerateMostLateStudentsReport(viewModel);
                return File(pdfBytes, "application/pdf", $"أكثر_الطلاب_تأخر_{fromDate:yyyy-MM-dd}_إلى_{toDate:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // تقارير الطلاب الأكثر انضباطا
        [HttpGet]
        public IActionResult MostDisciplinedStudents()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetMostDisciplinedStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes"
                                 && (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value)
                                 && (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes"
                                && studentIds.Contains(a.Student_ID)
                                && a.Attendance_Date.Date >= startDate
                                && a.Attendance_Date.Date <= endDate
                ).ToList();

                var totalDays = (endDate - startDate).Days + 1;

                var result = students.Select(s =>
                {
                    var records = attendanceRecords.Where(r => r.Student_ID == s.Student_ID).ToList();
                    var presentDays = records.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر");
                    var lateDays = records.Count(r => r.Attendance_Status == "متأخر");
                    var absentDays = totalDays - presentDays;


                    var disciplinePercentage = totalDays > 0 ? Math.Round((presentDays * 100.0) / totalDays, 2) : 0;

                    return new MostDisciplinedStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom?.Class?.Class_Name ?? "غير محدد",
                        ClassRoomName = s.ClassRoom?.ClassRoom_Name ?? "غير محدد",
                        PresentDays = presentDays,
                        LateDays = lateDays,
                        AbsentDays = absentDays,
                        TotalDays = totalDays,
                        DisciplinePercentage = disciplinePercentage,
                        AbsencePercentage = totalDays > 0 ? Math.Round((absentDays * 100.0) / totalDays, 2) : 0,
                        LatePercentage = totalDays > 0 ? Math.Round((lateDays * 100.0) / totalDays, 2) : 0
                    };
                })
                .Where(s => s.PresentDays > 0)
                .OrderByDescending(s => s.DisciplinePercentage)
                .ThenByDescending(s => s.PresentDays)
                .Take(top)
                .ToList();

                var viewModel = new MostDisciplinedStudentsReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    ClassRoomId = classRoomId,
                    TopCount = top,
                    Students = result,
                    TotalStudents = result.Count,
                    TotalPresentDays = result.Sum(s => s.PresentDays),
                    PeriodText = $"من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}"
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintMostDisciplinedStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                var students = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes" &&
                                 (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value) &&
                                 (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes" &&
                                 studentIds.Contains(a.Student_ID) &&
                                 a.Attendance_Date.Date >= startDate &&
                                 a.Attendance_Date.Date <= endDate
                ).ToList();

                var totalDays = (endDate - startDate).Days + 1;

                var result = students.Select(s =>
                {
                    var records = attendanceRecords.Where(r => r.Student_ID == s.Student_ID).ToList();
                    var presentDays = records.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر");
                    var lateDays = records.Count(r => r.Attendance_Status == "متأخر");
                    var absentDays = totalDays - presentDays;

                    var disciplinePercentage = totalDays > 0 ? Math.Round((presentDays * 100.0) / totalDays, 2) : 0;

                    return new MostDisciplinedStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom?.Class?.Class_Name ?? "غير محدد",
                        ClassRoomName = s.ClassRoom?.ClassRoom_Name ?? "غير محدد",
                        PresentDays = presentDays,
                        LateDays = lateDays,
                        AbsentDays = absentDays,
                        TotalDays = totalDays,
                        DisciplinePercentage = disciplinePercentage,
                        AbsencePercentage = totalDays > 0 ? Math.Round((absentDays * 100.0) / totalDays, 2) : 0,
                        LatePercentage = totalDays > 0 ? Math.Round((lateDays * 100.0) / totalDays, 2) : 0
                    };
                })
                .Where(s => s.PresentDays > 0)
                .OrderByDescending(s => s.DisciplinePercentage)
                .ThenByDescending(s => s.PresentDays)
                .Take(top)
                .ToList();

                var viewModel = new MostDisciplinedStudentsReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    ClassRoomId = classRoomId,
                    TopCount = top,
                    Students = result,
                    TotalStudents = result.Count,
                    TotalPresentDays = result.Sum(s => s.PresentDays),
                    PeriodText = $"من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}"
                };

                var pdfBytes = _pdfService.GenerateMostDisciplinedStudentsReport(viewModel);
                return File(pdfBytes, "application/pdf", $"الطلاب_الأكثر_انضباطا_{fromDate:yyyy-MM-dd}_إلى_{toDate:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //  تقرير الفصول الأكثر انضباطا
        [HttpGet]
        public IActionResult MostDisciplinedClasses()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetMostDisciplinedClassesReport(DateTime? fromDate, DateTime? toDate, int? classId)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var totalDays = (endDate - startDate).Days + 1;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes" &&
                                  (!classId.HasValue || cr.Class_ID == classId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var studentIds = classRooms.SelectMany(cr => cr.Students.Where(s => s.Student_Visible == "yes"))
                                           .Select(s => s.Student_ID)
                                           .ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes" &&
                                 a.Attendance_Date.Date >= startDate &&
                                 a.Attendance_Date.Date <= endDate &&
                                 studentIds.Contains(a.Student_ID)
                ).ToList();

                var result = classRooms.Select(cr =>
                {
                    var classStudents = cr.Students.Where(s => s.Student_Visible == "yes").ToList();
                    var classStudentIds = classStudents.Select(s => s.Student_ID).ToList();
                    var classRecords = attendanceRecords.Where(a => classStudentIds.Contains(a.Student_ID)).ToList();

                    var totalPresentDays = classRecords.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر" || r.Attendance_Status == "استئذان");
                    var totalLateDays = classRecords.Count(r => r.Attendance_Status == "متأخر");
                    var totalAbsentDays = totalDays - totalPresentDays;

                    var totalPossibleDays = classStudents.Count * totalDays;
                    var disciplinePercentage = totalPossibleDays > 0 ? Math.Round((totalPresentDays * 100.0) / totalPossibleDays, 2) : 0;

                    return new MostDisciplinedClassViewModel
                    {
                        ClassId = cr.Class_ID,
                        ClassName = cr.Class?.Class_Name ?? "غير محدد",
                        ClassRoomId = cr.ClassRoom_ID,
                        ClassRoomName = cr.ClassRoom_Name,
                        TotalStudents = classStudents.Count,
                        TotalPresentDays = totalPresentDays,
                        TotalLateDays = totalLateDays,
                        TotalAbsentDays = totalAbsentDays,
                        DisciplinePercentage = disciplinePercentage,
                        AbsencePercentage = totalPossibleDays > 0 ? Math.Round((totalAbsentDays * 100.0) / totalPossibleDays, 2) : 0,
                        LatePercentage = totalPossibleDays > 0 ? Math.Round((totalLateDays * 100.0) / totalPossibleDays, 2) : 0
                    };
                })
                .Where(c => c.TotalStudents > 0)
                .OrderByDescending(c => c.DisciplinePercentage)
                .ThenByDescending(c => c.TotalPresentDays)
                .ToList();

                for (int i = 0; i < result.Count; i++)
                    result[i].Rank = i + 1;

                var viewModel = new MostDisciplinedClassesReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    Classes = result,
                    TotalClasses = result.Count,
                    TotalStudents = result.Sum(c => c.TotalStudents),
                    PeriodText = $"من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}"
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult PrintMostDisciplinedClassesPdf(DateTime fromDate, DateTime toDate, int? classId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var totalDays = (endDate - startDate).Days + 1;

                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    cr => cr.ClassRoom_Visible == "yes" && (!classId.HasValue || cr.Class_ID == classId.Value),
                    includeProperties: new[] { "Class", "Students" }
                ).ToList();

                var studentIds = classRooms.SelectMany(cr => cr.Students.Where(s => s.Student_Visible == "yes"))
                                           .Select(s => s.Student_ID)
                                           .ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a => a.Attendance_Visible == "yes" &&
                                 a.Attendance_Date.Date >= startDate &&
                                 a.Attendance_Date.Date <= endDate &&
                                 studentIds.Contains(a.Student_ID)
                ).ToList();

                var result = classRooms.Select(cr =>
                {
                    var classStudents = cr.Students.Where(s => s.Student_Visible == "yes").ToList();
                    var classStudentIds = classStudents.Select(s => s.Student_ID).ToList();
                    var classRecords = attendanceRecords.Where(a => classStudentIds.Contains(a.Student_ID)).ToList();

                    var totalPresentDays = classRecords.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر" || r.Attendance_Status == "استئذان");
                    var totalLateDays = classRecords.Count(r => r.Attendance_Status == "متأخر");
                    var totalAbsentDays = totalLateDays - totalLateDays;

                    var totalPossibleDays = classStudents.Count * totalDays;
                    var disciplinePercentage = totalPossibleDays > 0 ? Math.Round((totalPresentDays * 100.0) / totalPossibleDays, 2) : 0;

                    return new MostDisciplinedClassViewModel
                    {
                        ClassId = cr.Class_ID,
                        ClassName = cr.Class?.Class_Name ?? "غير محدد",
                        ClassRoomId = cr.ClassRoom_ID,
                        ClassRoomName = cr.ClassRoom_Name,
                        TotalStudents = classStudents.Count,
                        TotalPresentDays = totalPresentDays,
                        TotalLateDays = totalLateDays,
                        TotalAbsentDays = totalAbsentDays,
                        DisciplinePercentage = disciplinePercentage,
                        AbsencePercentage = totalPossibleDays > 0 ? Math.Round((totalAbsentDays * 100.0) / totalPossibleDays, 2) : 0,
                        LatePercentage = totalPossibleDays > 0 ? Math.Round((totalLateDays * 100.0) / totalPossibleDays, 2) : 0
                    };
                })
                .Where(c => c.TotalStudents > 0)
                .OrderByDescending(c => c.DisciplinePercentage)
                .ThenByDescending(c => c.TotalPresentDays)
                .ToList();

                for (int i = 0; i < result.Count; i++)
                    result[i].Rank = i + 1;

                var viewModel = new MostDisciplinedClassesReportViewModel
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    ClassId = classId,
                    Classes = result,
                    TotalClasses = result.Count,
                    TotalStudents = result.Sum(c => c.TotalStudents),
                    PeriodText = $"من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}"
                };

                var pdfBytes = _pdfService.GenerateMostDisciplinedClassesReport(viewModel);
                return File(pdfBytes, "application/pdf", $"الفصول_الأكثر_انضباطا_{fromDate:yyyy-MM-dd}_إلى_{toDate:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }


        // التأخر والغياب حسب يوم فب الاسبوع

        [HttpGet]
        public async Task<IActionResult> WeeklyLatePatternReport()
        {
            ViewBag.ReportType = "late";
            ViewBag.ReportTitle = "تقرير التأخر الأسبوعية";

            var classes = _unitOfWork.TblClass.GetAll(
                c => c.Class_Visible == "yes",
                orderBy: c => c.Class_Name
            )
            .Select(c => new { id = c.Class_ID, name = c.Class_Name })
            .ToList();

            ViewBag.Classes = classes;

            return View("WeeklyPatternReport");
        }


        [HttpGet]
        public IActionResult WeeklyAbsentPatternReport()
        {
            ViewBag.ReportType = "absent";
            ViewBag.ReportTitle = "تقرير الغياب الأسبوعية";

            var classes = _unitOfWork.TblClass.GetAll(
                c => c.Class_Visible == "yes",
                orderBy: c => c.Class_Name
            )
            .Select(c => new { id = c.Class_ID, name = c.Class_Name })
            .ToList();

            ViewBag.Classes = classes;

            return View("WeeklyPatternReport");
        }



        public async Task<IActionResult> GetWeeklyPatternReportStrict(DateTime startDate, DateTime endDate, int? classId, int? classRoomId, string reportType = "both")
        {
            try
            {
                if (startDate >= endDate)
                    return Json(new { success = false, message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });

                int totalWeeks = (int)Math.Ceiling((endDate - startDate).TotalDays / 7.0);

                if (totalWeeks < 2)
                    return Json(new { success = false, message = "الفترة يجب أن تشمل أسبوعين على الأقل" });

                var students = _unitOfWork.TblStudent.GetAll(
                    s =>
                        s.Student_Visible == "yes" &&
                        (!classId.HasValue || s.ClassRoom.Class_ID == classId.Value) &&
                        (!classRoomId.HasValue || s.ClassRoom_ID == classRoomId.Value),
                    includeProperties: new[] { "ClassRoom", "ClassRoom.Class" }
                ).ToList();

                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = _unitOfWork.TblAttendance.GetAll(
                    a =>
                        a.Attendance_Visible == "yes" &&
                        studentIds.Contains(a.Student_ID) &&
                        a.Attendance_Date >= startDate &&
                        a.Attendance_Date <= endDate,
                    includeProperties: null,
                    orderBy: null
                ).ToList();


                var arabicDays = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Sunday, "الأحد" },
            { DayOfWeek.Monday, "الإثنين" },
            { DayOfWeek.Tuesday, "الثلاثاء" },
            { DayOfWeek.Wednesday, "الأربعاء" },
            { DayOfWeek.Thursday, "الخميس" },
            { DayOfWeek.Friday, "الجمعة" },
            { DayOfWeek.Saturday, "السبت" }
        };

                var result = new List<StudentWeeklyPatternViewModel>();

                foreach (var student in students)
                {
                    var studentRecords = attendanceRecords
                        .Where(r => r.Student_ID == student.Student_ID)
                        .ToList();

                    if (!studentRecords.Any())
                        continue;

                    var dayWeekCounts = new Dictionary<DayOfWeek, int>();

                    foreach (var record in studentRecords)
                    {
                        if (reportType == "late" && record.Attendance_Status != "متأخر") continue;
                        if (reportType == "absent" && record.Attendance_Status != "غياب") continue;
                        if (reportType == "both" && record.Attendance_Status != "غياب" && record.Attendance_Status != "متأخر") continue;

                        var day = record.Attendance_Date.DayOfWeek;

                        if (!dayWeekCounts.ContainsKey(day))
                            dayWeekCounts[day] = 0;

                        dayWeekCounts[day]++;
                    }

                    var repeatedDays = dayWeekCounts.Where(d => d.Value == totalWeeks).ToList();
                    if (!repeatedDays.Any()) continue;

                    var patterns = repeatedDays
                        .Select(day => new DayPatternViewModel
                        {
                            DayName = arabicDays[day.Key],
                            DayNameEnglish = day.Key.ToString(),
                            LateCount = studentRecords.Count(r => r.Attendance_Date.DayOfWeek == day.Key && r.Attendance_Status == "متأخر"),
                            AbsentCount = studentRecords.Count(r => r.Attendance_Date.DayOfWeek == day.Key && r.Attendance_Status == "غياب"),
                            TotalOccurrences = totalWeeks,
                            Percentage = 100,
                            PatternType = reportType == "late" ? "late" :
                                          reportType == "absent" ? "absent" : "mixed"
                        })
                        .ToList();

                    result.Add(new StudentWeeklyPatternViewModel
                    {
                        StudentId = student.Student_ID,
                        StudentName = student.Student_Name,
                        StudentCode = student.Student_Code,
                        ClassName = student.ClassRoom?.Class?.Class_Name,
                        ClassRoomName = student.ClassRoom?.ClassRoom_Name,
                        DayPatterns = patterns
                    });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private WeeklySummaryViewModel CreateWeeklySummary(List<StudentWeeklyPatternViewModel> students, string reportType)
        {
            var strongPatternStudents = students
                .Where(r => r.PatternStrength >= 30)
                .OrderByDescending(r => r.PatternStrength)
                .Take(10)
                .Select(r => new StrongPatternStudentViewModel
                {
                    StudentId = r.StudentId,
                    StudentName = r.StudentName,
                    StudentCode = r.StudentCode,
                    PatternDay = r.MostFrequentDay,
                    PatternType = r.MostFrequentType,
                    OccurrenceCount = r.DayPatterns.First(d => d.DayName == r.MostFrequentDay).TotalOccurrences,
                    Percentage = r.PatternStrength
                })
                .ToList();

            return new WeeklySummaryViewModel
            {
                TotalStudents = students.Count > 0 ? students.First().TotalStudents : 0,
                StudentsWithPatterns = students.Count,
                DayPatternsCount = students.GroupBy(r => r.MostFrequentDay)
                    .ToDictionary(g => g.Key, g => g.Count()),
                MostFrequentDays = students.GroupBy(r => r.MostFrequentDay)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count()),
                StrongPatternStudents = strongPatternStudents
            };
        }


        
        [HttpGet]
        public IActionResult AbsenceNotice()
        {
            var students = _unitOfWork.TblStudent.GetAll(
                s => s.Student_Visible == "yes"
            ).Select(s => new SelectListItem
            {
                Value = s.Student_ID.ToString(),
                Text = s.Student_Name
            })
            .ToList();

            ViewBag.Students = students;
            return View();
        }


        [HttpGet]
        public IActionResult GenerateAbsenceNotice(string studentCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Code == studentCode,
                    includeProperties: new[] { "ClassRoom.Class" }
                ).FirstOrDefault();

                if (student == null)
                    return NotFound("الطالبة غير موجودة");


                if (fromDate == null)
                {
                    var firstRecord = _unitOfWork.TblAttendance.GetAll(
                        a => a.Student_ID == student.Student_ID,
                        orderBy: a => a.Attendance_Date
                    ).FirstOrDefault();

                    fromDate = firstRecord?.Attendance_Date ?? DateTime.Today;
                }

                toDate ??= DateTime.Today;


                var allRecords = _unitOfWork.TblAttendance.GetAll(
                    a =>
                        a.Student_ID == student.Student_ID &&
                        a.Attendance_Visible == "yes" &&
                        a.Attendance_Date >= fromDate &&
                        a.Attendance_Date <= toDate
                ).ToList();


                var groupedDates = allRecords
                    .GroupBy(r => r.Attendance_Date.Date)
                    .ToList();

                var absenceDates = new List<AbsenceDateInfo>();

                foreach (var dayGroup in groupedDates)
                {
                    var recordsInDay = dayGroup.ToList();

                    bool hasPresent = recordsInDay.Any(r =>
                        r.Attendance_Status == "حضور" ||
                        r.Attendance_Status == "متأخر" ||
                        r.Attendance_Status == "استئذان"
                    );

                    if (hasPresent)
                        continue; 

                    var day = dayGroup.Key;

                    absenceDates.Add(new AbsenceDateInfo
                    {
                        Date = day.ToString("dd/MM/yyyy"),
                        DayName = day.ToString("dddd", new System.Globalization.CultureInfo("ar-SA"))
                    });
                }

                absenceDates = absenceDates
                    .OrderBy(x => DateTime.ParseExact(x.Date, "dd/MM/yyyy", null))
                    .ToList();

                int row = 1;
                absenceDates.ForEach(a => a.RowNumber = row++);

                var noticeData = new StudentAbsenceNoticeViewModel
                {
                    StudentName = student.Student_Name,
                    ClassName = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    StudentGuardianType = "ولي الأمر",
                    AbsenceDates = absenceDates,
                    NoticeText = "",
                    DateText = DateTime.Now.ToString("dd/MM/yyyy"),
                    FromDate = fromDate?.ToString("dd/MM/yyyy"),
                    ToDate = toDate?.ToString("dd/MM/yyyy")
                };


                var pdfBytes = _pdfService.GenerateStudentAbsenceNotice(noticeData, _academicYear, _semester);

                return File(pdfBytes, "application/pdf",
                    $"إخطار_غياب_{student.Student_Name}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"حدث خطأ: {ex.Message}");
            }
        }


    }
}


