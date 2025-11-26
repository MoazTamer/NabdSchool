using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using SalesModel.Models;
using SalesModel.ViewModels.Reports;
using SalesRepository.Data;
using SalesRepository.Repository;

namespace Sales.Controllers
{
    public class ReportsController : Controller
    {
        private readonly SalesDBContext _context;
        private readonly ReportPdfService _pdfService;

        public ReportsController(SalesDBContext context, ReportPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            try
            {
                var classes = await _context.TblClass
                    .Where(c => c.Class_Visible == "yes")
                    .Select(c => new { id = c.Class_ID, name = c.Class_Name })
                    .ToListAsync();

                return Json(new { success = true, data = classes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClassRooms(int classId)
        {
            try
            {
                var classRooms = await _context.TblClassRoom
                    .Where(cr => cr.Class_ID == classId && cr.ClassRoom_Visible == "yes")
                    .OrderBy(cr => cr.ClassRoom_Name)
                    .Select(cr => new { id = cr.ClassRoom_ID, name = cr.ClassRoom_Name })
                    .ToListAsync();

                return Json(new { success = true, data = classRooms });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DailyAbsence()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyAbsenceReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;

                // 1) Load classrooms + students
                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0) query = query.Where(cr => cr.Class_ID == classId.Value);
                if (classRoomId.HasValue && classRoomId.Value > 0) query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                // 2) Today's attendance (present/late) set — نحتاجها فقط لتحديد الحاضرين اليوم
                var todayAttendanceStudentIds = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate.Date &&
                        a.Attendance_Visible == "yes")
                    .Select(a => new { a.Student_ID, a.Attendance_Status })
                    .ToListAsync();

                var presentTodaySet = todayAttendanceStudentIds
                    .Where(x => x.Attendance_Status == "حضور" || x.Attendance_Status == "متأخر")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                // لو مفيش ولا student عنده سجل في هذا اليوم → اليوم غير مسجل حضور/غياب
                if (!todayAttendanceStudentIds.Any())
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


                // 3) Get all attendance records (any status) for last 30 days for all relevant students in one query
                var minDate = reportDate.AddDays(-30);
                var relevantStudentIds = classRooms.SelectMany(cr => cr.Students)
                                                   .Where(s => s.Student_Visible == "yes")
                                                   .Select(s => s.Student_ID)
                                                   .Distinct()
                                                   .ToList();

                var last30 = await _context.TblAttendance
                .Where(a =>
                    a.Attendance_Date.Date >= minDate &&
                    a.Attendance_Date.Date <= reportDate &&
                    a.Attendance_Visible == "yes" &&
                    relevantStudentIds.Contains(a.Student_ID))
                .Select(a => new
                {
                    a.Student_ID,
                    Date = a.Attendance_Date.Date,
                    Status = a.Attendance_Status,
                    Time = a.Attendance_Time 
                })
                .ToListAsync();

                            var studentDateStatus = last30
                                .GroupBy(a => a.Student_ID)
                                .ToDictionary(
                                    g => g.Key,
                                    g => g
                                        .GroupBy(x => x.Date)
                                        .ToDictionary(
                                            gg => gg.Key,
                                            gg => gg
                                                .OrderByDescending(x => x.Time)
                                                .First()
                                                .Status
                                        )
                                );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students.Where(s => s.Student_Visible == "yes").ToList();
                    var studentIds = activeStudents.Select(s => s.Student_ID).ToList();

                    var absentIds = studentIds.Where(id => !presentTodaySet.Contains(id)).ToList();
                    if (!absentIds.Any()) continue;

                    var absentList = new List<AbsentStudentViewModel>();

                    foreach (var sid in absentIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0;
                        int currentStreak = 0;

                        // نمشي من أقدم يوم إلى أحدث يوم
                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "غياب", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    if (currentStreak > maxConsecutive)
                                        maxConsecutive = currentStreak;
                                }
                                else
                                {
                                    currentStreak = 0;
                                }
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
                        AbsencePercentage = activeStudents.Count > 0
                            ? Math.Round((decimal)absentIds.Count / activeStudents.Count * 100, 2)
                            : 0,
                        AbsentStudentsList = absentList.OrderBy(s => s.StudentName).ToList()
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence.OrderBy(c => c.ClassName).ThenBy(c => c.ClassRoomName).ToList(),
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
        public async Task<IActionResult> PrintDailyAbsencePdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                // جمع كل الطلاب المعنيين (مرة واحدة)
                var relevantStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                // جلب سجلات الحضور لليوم المطلوب (حضور/متأخر) لتحديد الحاضرين فعلياً
                var todayRecords = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new { a.Student_ID, a.Attendance_Status })
                    .ToListAsync();

                var presentTodaySet = todayRecords
                    .Where(x => x.Attendance_Status == "حضور" || x.Attendance_Status == "متأخر")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                // نجيب كل السجلات لآخر 30 يوم للطلاب المعنيين (نستخدم نفس الفترة حتى نقدر نعرف التتابع)
                var minDate = reportDate.AddDays(-30);

                var last30 = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        Date = a.Attendance_Date.Date,
                        Status = a.Attendance_Status,
                        Time = a.Attendance_Time // نفترض هذا الحقل موجود ويمكن الترتيب بواسطته لاختيار أحدث سجل لنفس اليوم
                    })
                    .ToListAsync();

                // بناء خريطة: studentId -> (date -> آخر حالة في ذلك اليوم)
                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .GroupBy(x => x.Date)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg
                                    .OrderByDescending(x => x.Time) // نأخذ أحدث سجل لذلك اليوم
                                    .First()
                                    .Status
                            )
                    );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students?.Where(s => s.Student_Visible == "yes").ToList() ?? new List<TblStudent>();
                    var studentIds = activeStudents.Select(s => s.Student_ID).ToList();

                    // الطلاب الغائبين اليوم = الموجودين في الفصل لكن ليسوا في مجموعة الحضور اليوم
                    var absentStudentIds = studentIds.Where(id => !presentTodaySet.Contains(id)).ToList();
                    if (!absentStudentIds.Any()) continue;

                    var absentStudentsList = new List<AbsentStudentViewModel>();

                    foreach (var sid in absentStudentIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0;
                        int currentStreak = 0;

                        // نمشي من أقدم يوم إلى أحدث يوم
                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "غياب", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    if (currentStreak > maxConsecutive)
                                        maxConsecutive = currentStreak;
                                }
                                else
                                {
                                    currentStreak = 0;
                                }
                            }
                            else
                            {
                                currentStreak = 0;
                            }
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        absentStudentsList.Add(new AbsentStudentViewModel
                        {
                            StudentId = st.Student_ID,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = maxConsecutive,
                            Notes = maxConsecutive >= 3 ? "تحذير: غياب متكرر" : ""
                        });
                    }

                    var absencePercentage = activeStudents.Count > 0
                        ? Math.Round((decimal)absentStudentIds.Count / activeStudents.Count * 100, 2)
                        : 0;

                    classesAbsence.Add(new ClassAbsenceViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name ?? "غير محدد",
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = activeStudents.Count,
                        AbsentStudents = absentStudentIds.Count,
                        AbsencePercentage = absencePercentage,
                        AbsentStudentsList = absentStudentsList.OrderBy(s => s.StudentName).ToList()
                    });
                }

                var viewModel = new DailyAbsenceReportViewModel
                {
                    ReportDate = reportDate,
                    ClassesAbsence = classesAbsence.OrderBy(c => c.ClassName).ThenBy(c => c.ClassRoomName).ToList(),
                    TotalAbsentStudents = classesAbsence.Sum(c => c.AbsentStudents),
                    TotalClasses = classesAbsence.Count
                };

                var pdfBytes = _pdfService.GenerateDailyAbsenceReport(viewModel);
                return File(pdfBytes, "application/pdf", $"تقرير_الغياب_اليومي_{date:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // تقرير الخروج المبكر
        [HttpGet]
        public IActionResult DailyEarlyExit()
        {
            return View();
        }

        public async Task<IActionResult> GetDailyEarlyExitReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;

                // 1) Load classrooms + students
                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                // كل الطلاب المعنيين
                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students)
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                // 2) Attendance of the same day — نحتاج فقط "خروج مبكر"
                var todayRecords = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        allStudentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        a.Attendance_Status,
                        a.Attendance_Time
                    })
                    .ToListAsync();

                // الطلاب اللي خرجوا بدري اليوم
                var earlyExitSet = todayRecords
                    .Where(x => x.Attendance_Status == "استئذان")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                var classesReport = new List<ClassEarlyExitViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var students = classRoom.Students
                        .Where(s => s.Student_Visible == "yes")
                        .ToList();

                    var studentIds = students.Select(s => s.Student_ID).ToList();

                    // الطلاب اللي لهم خروج مبكر من هذا الفصل
                    var earlyExitIds = studentIds
                        .Where(id => earlyExitSet.Contains(id))
                        .ToList();

                    if (!earlyExitIds.Any())
                        continue;

                    var exitList = new List<EarlyExitStudentViewModel>();

                    foreach (var sid in earlyExitIds)
                    {
                        var student = students.First(s => s.Student_ID == sid);

                        var time = todayRecords
                            .Where(r => r.Student_ID == sid && r.Attendance_Status == "استئذان")
                            .OrderByDescending(r => r.Attendance_Time)
                            .FirstOrDefault()?.Attendance_Time;

                        exitList.Add(new EarlyExitStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = student.Student_Name,
                            StudentCode = student.Student_Code,
                            StudentPhone = student.Student_Phone,
                            ExitTime = time?.ToString(@"hh\:mm") ?? ""
                        });
                    }

                    classesReport.Add(new ClassEarlyExitViewModel
                    {
                        ClassId = classRoom.Class_ID,
                        ClassName = classRoom.Class?.Class_Name,
                        ClassRoomId = classRoom.ClassRoom_ID,
                        ClassRoomName = classRoom.ClassRoom_Name,
                        TotalStudents = students.Count,
                        EarlyExitStudents = exitList.Count,
                        EarlyExitStudentsList = exitList.OrderBy(s => s.StudentName).ToList()
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
        public async Task<IActionResult> PrintDailyEarlyExitPdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                var allStudentIds = classRooms
                    .SelectMany(cr => cr.Students)
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                var todayRecords = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        allStudentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        a.Attendance_Status,
                        a.Attendance_Time
                    })
                    .ToListAsync();

                var earlyExitSet = todayRecords
                    .Where(x => x.Attendance_Status == "استئذان")
                    .Select(x => x.Student_ID)
                    .ToHashSet();

                var classesReport = new List<ClassEarlyExitViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var students = classRoom.Students
                        .Where(s => s.Student_Visible == "yes")
                        .ToList();

                    var earlyExitIds = students
                        .Where(s => earlyExitSet.Contains(s.Student_ID))
                        .Select(s => s.Student_ID)
                        .ToList();

                    if (!earlyExitIds.Any())
                        continue;

                    var exitList = earlyExitIds
                        .Select(id =>
                        {
                            var st = students.First(s => s.Student_ID == id);
                            var time = todayRecords
                                .Where(r => r.Student_ID == id && r.Attendance_Status == "استئذان")
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
                        })
                        .OrderBy(x => x.StudentName)
                        .ToList();

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


        // 
        public IActionResult DailyLate()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyLateReport(DateTime? date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date ?? DateTime.Today;

                // 1) Load classrooms + students
                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0) query = query.Where(cr => cr.Class_ID == classId.Value);
                if (classRoomId.HasValue && classRoomId.Value > 0) query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                // 2) Today's attendance (present/late) set - جلب معاد الحضور أيضاً
                var todayAttendance = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate.Date &&
                        a.Attendance_Visible == "yes")
                    .Select(a => new {
                        a.Student_ID,
                        a.Attendance_Status,
                        a.Attendance_Time // إضافة معاد الحضور
                    })
                    .ToListAsync();

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

                // الطلاب المتأخرين اليوم مع معاد الحضور
                var lateToday = todayAttendance
                    .Where(x => x.Attendance_Status == "متأخر")
                    .Select(x => new {
                        StudentId = x.Student_ID,
                        AttendanceTime = x.Attendance_Time
                    })
                    .ToList();

                var lateTodayIds = lateToday.Select(x => x.StudentId).ToList();

                // 3) Get all attendance records (any status) for last 30 days for all relevant students
                var minDate = reportDate.AddDays(-30);
                var relevantStudentIds = classRooms.SelectMany(cr => cr.Students)
                                                   .Where(s => s.Student_Visible == "yes")
                                                   .Select(s => s.Student_ID)
                                                   .Distinct()
                                                   .ToList();

                var last30 = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        Date = a.Attendance_Date.Date,
                        Status = a.Attendance_Status,
                        Time = a.Attendance_Time
                    })
                    .ToListAsync();

                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .GroupBy(x => x.Date)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg
                                    .OrderByDescending(x => x.Time)
                                    .First()
                                    .Status
                            )
                    );

                // إنشاء dictionary لمعاد الحضور اليوم
                var studentAttendanceTime = lateToday
                    .ToDictionary(x => x.StudentId, x => x.AttendanceTime);

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students.Where(s => s.Student_Visible == "yes").ToList();

                    var classLateIds = activeStudents
                        .Select(s => s.Student_ID)
                        .Where(id => lateTodayIds.Contains(id))
                        .ToList();

                    if (!classLateIds.Any()) continue;

                    var lateList = new List<AbsentStudentViewModel>();

                    foreach (var sid in classLateIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0;
                        int currentStreak = 0;

                        // loop from oldest day to newest within last 30 days
                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    if (currentStreak > maxConsecutive)
                                        maxConsecutive = currentStreak;
                                }
                                else
                                {
                                    currentStreak = 0;
                                }
                            }
                            else
                            {
                                currentStreak = 0;
                            }
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        // جلب معاد الحضور
                        TimeSpan? attendanceTime = null;
                        if (studentAttendanceTime.TryGetValue(sid, out var time))
                        {
                            attendanceTime = time;
                        }

                        lateList.Add(new AbsentStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            AttendanceTime = attendanceTime, // إضافة معاد الحضور
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
                    ClassesAbsence = classesAbsence.OrderBy(c => c.ClassName).ThenBy(c => c.ClassRoomName).ToList(),
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
        public async Task<IActionResult> PrintDailyLatePdf(DateTime date, int? classId, int? classRoomId)
        {
            try
            {
                var reportDate = date.Date;

                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(cr => cr.ClassRoom_ID == classRoomId.Value);

                var classRooms = await query.ToListAsync();

                // جمع كل الطلاب المعنيين
                var relevantStudentIds = classRooms
                    .SelectMany(cr => cr.Students ?? new List<TblStudent>())
                    .Where(s => s.Student_Visible == "yes")
                    .Select(s => s.Student_ID)
                    .Distinct()
                    .ToList();

                // جلب سجلات الحضور لليوم المطلوب (حضور/متأخر) لتحديد المتأخرين - مع إضافة معاد الحضور
                var todayRecords = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new {
                        a.Student_ID,
                        a.Attendance_Status,
                        a.Attendance_Time // إضافة معاد الحضور
                    })
                    .ToListAsync();

                // الطلاب المتأخرين اليوم مع معاد الحضور
                var lateToday = todayRecords
                    .Where(x => x.Attendance_Status == "متأخر")
                    .Select(x => new {
                        StudentId = x.Student_ID,
                        AttendanceTime = x.Attendance_Time
                    })
                    .ToList();

                var lateTodayIds = lateToday.Select(x => x.StudentId).ToList();

                // إنشاء dictionary لمعاد الحضور اليوم
                var studentAttendanceTime = lateToday
                    .ToDictionary(x => x.StudentId, x => x.AttendanceTime);

                // جلب كل سجلات آخر 30 يوم للطلاب المعنيين
                var minDate = reportDate.AddDays(-30);

                var last30 = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        Date = a.Attendance_Date.Date,
                        Status = a.Attendance_Status,
                        Time = a.Attendance_Time
                    })
                    .ToListAsync();

                var studentDateStatus = last30
                    .GroupBy(a => a.Student_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .GroupBy(x => x.Date)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg
                                    .OrderByDescending(x => x.Time)
                                    .First()
                                    .Status
                            )
                    );

                var classesAbsence = new List<ClassAbsenceViewModel>();

                foreach (var classRoom in classRooms)
                {
                    var activeStudents = classRoom.Students?.Where(s => s.Student_Visible == "yes").ToList() ?? new List<TblStudent>();

                    var classLateIds = activeStudents
                        .Select(s => s.Student_ID)
                        .Where(id => lateTodayIds.Contains(id))
                        .ToList();

                    if (!classLateIds.Any()) continue;

                    var lateList = new List<AbsentStudentViewModel>();

                    foreach (var sid in classLateIds)
                    {
                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        int maxConsecutive = 0;
                        int currentStreak = 0;

                        // loop from oldest day to newest within last 30 days
                        for (int i = 30; i >= 0; i--)
                        {
                            var day = reportDate.AddDays(-i).Date;

                            if (dateStatusMap != null && dateStatusMap.TryGetValue(day, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    currentStreak++;
                                    if (currentStreak > maxConsecutive)
                                        maxConsecutive = currentStreak;
                                }
                                else
                                {
                                    currentStreak = 0;
                                }
                            }
                            else
                            {
                                currentStreak = 0;
                            }
                        }


                        var st = activeStudents.First(s => s.Student_ID == sid);

                        // جلب معاد الحضور
                        TimeSpan? attendanceTime = null;
                        if (studentAttendanceTime.TryGetValue(sid, out var time))
                        {
                            attendanceTime = time;
                        }

                        lateList.Add(new AbsentStudentViewModel
                        {
                            StudentId = st.Student_ID,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            AttendanceTime = attendanceTime, // إضافة معاد الحضور
                            ConsecutiveAbsenceDays = maxConsecutive,
                            Notes = maxConsecutive >= 3 ? "تحذير: تأخر متكرر" : ""
                        });
                    }

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
                        AbsentStudentsList = lateList.OrderBy(s => s.StudentName).ToList()
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

        public IActionResult StudentReport(string studentIdentifier, DateTime? fromDate, DateTime? date)
        {
            ViewBag.StudentIdentifier = studentIdentifier;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Date = date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetStudentAttendanceReport(string studentIdentifier, DateTime? date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;
                //var minDate = reportDate;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);
                //var minDate = fromDate?.Date ?? reportDate.AddDays(-30);
                //var minDate = reportDate.AddDays(-30);

                //var code = studentIdentifier.Trim();

                studentIdentifier = studentIdentifier.Trim();

                var student = await _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => (s.Student_Code == studentIdentifier || EF.Functions.Like(s.Student_Name, $"%{studentIdentifier}%"))
                                && s.Student_Visible == "yes")
                    .FirstOrDefaultAsync();




                if (student == null)
                    return Json(new { success = false, message = "الطالبة غير موجود" });

                // جلب سجلات آخر 30 يوم
                var records = await _context.TblAttendance
                    .Where(a =>
                        a.Student_ID == student.Student_ID &&
                        a.Attendance_Visible == "yes" &&
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate)
                    .Select(a => new
                    {
                        Date = a.Attendance_Date.Date,
                        a.Attendance_Status,
                        a.Attendance_Time
                    })
                    .ToListAsync();

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


                // حساب الإحصائيات
                int present = result.Count(r => r.Status == "حضور" || r.Status == "متأخر");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                // حساب التأخر المتتالي
                int maxConsecutiveLate = 0;
                int currentLateCount = 0;

                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر")
                    {
                        currentLateCount++;
                        if (currentLateCount > maxConsecutiveLate)
                            maxConsecutiveLate = currentLateCount;
                    }
                    else
                    {
                        currentLateCount = 0;
                    }
                }


                // حساب الغياب المتتالي
                int maxConsecutiveAbsent = 0;
                int currentAbsentCount = 0;

                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب")
                    {
                        currentAbsentCount++;
                        if (currentAbsentCount > maxConsecutiveAbsent)
                            maxConsecutiveAbsent = currentAbsentCount;
                    }
                    else
                    {
                        currentAbsentCount = 0;
                    }
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
        public async Task<IActionResult> PrintStudentAttendancePdf(string studentIdentifier, DateTime date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date.Date;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);
                //var minDate = reportDate.AddDays(-30);

                studentIdentifier = studentIdentifier.Trim();

                var student = await _context.TblStudent
                   .Include(s => s.ClassRoom)
                   .ThenInclude(cr => cr.Class)
                   .Where(s => (s.Student_Code == studentIdentifier || EF.Functions.Like(s.Student_Name, $"%{studentIdentifier}%"))
                               && s.Student_Visible == "yes")
                   .FirstOrDefaultAsync();


                if (student == null)
                {
                    return Json(new { success = false, message = "الطالبة غير موجودة" });
                }

                var records = await _context.TblAttendance
                    .Where(a =>
                        a.Student_ID == student.Student_ID &&
                        a.Attendance_Visible == "yes" &&
                        a.Attendance_Date.Date >= minDate &&
                        a.Attendance_Date.Date <= reportDate)
                    .Select(a => new
                    {
                        Date = a.Attendance_Date.Date,
                        a.Attendance_Status,
                        a.Attendance_Time
                    })
                    .ToListAsync();

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

                // حساب الإحصائيات
                int present = result.Count(r => r.Status == "حضور" || r.Status == "متأخر");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                // حساب التأخر المتتالي
                int maxConsecutiveLate = 0;
                int currentLateCount = 0;

                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر")
                    {
                        currentLateCount++;
                        if (currentLateCount > maxConsecutiveLate)
                            maxConsecutiveLate = currentLateCount;
                    }
                    else
                    {
                        currentLateCount = 0;
                    }
                }


                // حساب الغياب المتتالي
                int maxConsecutiveAbsent = 0;
                int currentAbsentCount = 0;

                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب")
                    {
                        currentAbsentCount++;
                        if (currentAbsentCount > maxConsecutiveAbsent)
                            maxConsecutiveAbsent = currentAbsentCount;
                    }
                    else
                    {
                        currentAbsentCount = 0;
                    }
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
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public IActionResult MostAbsentStudents()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMostAbsentStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                // 1️⃣ استعلام واحد للطلاب مع الغياب
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                // 2️⃣ join مع الحضور وحساب الغياب لكل طالب في استعلام واحد
                var result = await query
                    .Select(s => new MostAbsentStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom.Class.Class_Name,
                        ClassRoomName = s.ClassRoom.ClassRoom_Name,
                        AbsentDays = _context.TblAttendance
                            .Count(a => a.Student_ID == s.Student_ID &&
                                        a.Attendance_Visible == "yes" &&
                                        a.Attendance_Status == "غياب" &&
                                        a.Attendance_Date.Date >= startDate &&
                                        a.Attendance_Date.Date <= endDate),
                        TotalDays = (endDate - startDate).Days + 1
                    })
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToListAsync();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrintMostAbsentStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                // استعلام واحد للطلاب مع الغياب
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                // join مع الحضور وحساب الغياب لكل طالب في استعلام واحد
                var result = await query
                    .Select(s => new MostAbsentStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom.Class.Class_Name,
                        ClassRoomName = s.ClassRoom.ClassRoom_Name,
                        AbsentDays = _context.TblAttendance
                            .Count(a => a.Student_ID == s.Student_ID &&
                                        a.Attendance_Visible == "yes" &&
                                        a.Attendance_Status == "غياب" &&
                                        a.Attendance_Date.Date >= startDate &&
                                        a.Attendance_Date.Date <= endDate),
                        TotalDays = (endDate - startDate).Days + 1
                    })
                    .Where(s => s.AbsentDays > 0) // فقط الطلاب الذين لديهم غياب
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToListAsync();

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
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public IActionResult MostLateStudents()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMostLateStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                // 1️⃣ استعلام واحد للطلاب مع التأخر
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                // 2️⃣ join مع الحضور وحساب التأخر لكل طالب في استعلام واحد
                var result = await query
                    .Select(s => new MostAbsentStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom.Class.Class_Name,
                        ClassRoomName = s.ClassRoom.ClassRoom_Name,
                        AbsentDays = _context.TblAttendance
                            .Count(a => a.Student_ID == s.Student_ID &&
                                        a.Attendance_Visible == "yes" &&
                                        a.Attendance_Status == "متأخر" &&
                                        a.Attendance_Date.Date >= startDate &&
                                        a.Attendance_Date.Date <= endDate),
                        TotalDays = (endDate - startDate).Days + 1
                    })
                    .Where(s => s.AbsentDays > 0) // ✅ مهم: نعرض فقط الطلاب اللي عندهم تأخر
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToListAsync();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrintMostLateStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                // استعلام واحد للطلاب مع الغياب
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                // join مع الحضور وحساب الغياب لكل طالب في استعلام واحد
                var result = await query
                    .Select(s => new MostAbsentStudentViewModel
                    {
                        StudentId = s.Student_ID,
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        ClassName = s.ClassRoom.Class.Class_Name,
                        ClassRoomName = s.ClassRoom.ClassRoom_Name,
                        AbsentDays = _context.TblAttendance
                            .Count(a => a.Student_ID == s.Student_ID &&
                                        a.Attendance_Visible == "yes" &&
                                        a.Attendance_Status == "متأخر" &&
                                        a.Attendance_Date.Date >= startDate &&
                                        a.Attendance_Date.Date <= endDate),
                        TotalDays = (endDate - startDate).Days + 1
                    })
                    .Where(s => s.AbsentDays > 0) // فقط الطلاب الذين لديهم غياب
                    .OrderByDescending(s => s.AbsentDays)
                    .ThenBy(s => s.StudentName)
                    .Take(top)
                    .ToListAsync();

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
                Response.ContentType = "application/json";
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        //================= تقارير الطلاب الأكثر انضباطا =================//
        [HttpGet]
        public IActionResult MostDisciplinedStudents()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMostDisciplinedStudentsReport(DateTime? fromDate, DateTime? toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;
                var top = topCount ?? 10;

                // استعلام للطلاب
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                var students = await query.ToListAsync();
                var studentIds = students.Select(s => s.Student_ID).ToList();

                // جلب سجلات الحضور للفترة المحددة
                var attendanceRecords = await _context.TblAttendance
                    .Where(a => a.Attendance_Date.Date >= startDate &&
                                a.Attendance_Date.Date <= endDate &&
                                a.Attendance_Visible == "yes" &&
                                studentIds.Contains(a.Student_ID))
                    .GroupBy(a => a.Student_ID)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        PresentDays = g.Count(x => x.Attendance_Status == "حضور" || x.Attendance_Status ==  "متأخر"),
                        LateDays = g.Count(x => x.Attendance_Status == "متأخر"),
                        AbsentDays = g.Count(x => x.Attendance_Status == "غياب")
                    })
                    .ToListAsync();

                var totalDays = (endDate - startDate).Days + 1;

                var result = students.Select(s =>
                {
                    var records = attendanceRecords.FirstOrDefault(r => r.StudentId == s.Student_ID);
                    var presentDays = records?.PresentDays ?? 0;
                    var lateDays = records?.LateDays ?? 0;
                    var absentDays = records?.AbsentDays ?? 0;

                    // نسبة الانضباط = (أيام الحضور ÷ إجمالي الأيام) × 100
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
                .Where(s => s.PresentDays > 0) // فقط الطلاب الذين لديهم أيام حضور
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
        public async Task<IActionResult> PrintMostDisciplinedStudentsPdf(DateTime fromDate, DateTime toDate, int? classId, int? classRoomId, int? topCount = 10)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;
                var top = topCount ?? 10;

                // نفس منطق جلب البيانات
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue && classRoomId.Value > 0)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                var students = await query.ToListAsync();
                var studentIds = students.Select(s => s.Student_ID).ToList();

                var attendanceRecords = await _context.TblAttendance
                    .Where(a => a.Attendance_Date.Date >= startDate &&
                                a.Attendance_Date.Date <= endDate &&
                                a.Attendance_Visible == "yes" &&
                                studentIds.Contains(a.Student_ID))
                    .GroupBy(a => a.Student_ID)
                    .Select(g => new
                    {
                        StudentId = g.Key,
                        PresentDays = g.Count(x => x.Attendance_Status == "حضور" || x.Attendance_Status == "متأخر"),
                        LateDays = g.Count(x => x.Attendance_Status == "متأخر"),
                        AbsentDays = g.Count(x => x.Attendance_Status == "غياب")
                    })
                    .ToListAsync();

                var totalDays = (endDate - startDate).Days + 1;

                var result = students.Select(s =>
                {
                    var records = attendanceRecords.FirstOrDefault(r => r.StudentId == s.Student_ID);
                    var presentDays = records?.PresentDays ?? 0;
                    var lateDays = records?.LateDays ?? 0;
                    var absentDays = records?.AbsentDays ?? 0;

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

        [HttpGet]
        public IActionResult MostDisciplinedClasses()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMostDisciplinedClassesReport(DateTime? fromDate, DateTime? toDate, int? classId)
        {
            try
            {
                var startDate = fromDate?.Date ?? DateTime.Today.AddDays(-30);
                var endDate = toDate?.Date ?? DateTime.Today;

                // استعلام للفصول
                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                var classRooms = await query.ToListAsync();

                var classRoomIds = classRooms.Select(cr => cr.ClassRoom_ID).ToList();
                var studentIds = classRooms.SelectMany(cr => cr.Students.Where(s => s.Student_Visible == "yes"))
                                          .Select(s => s.Student_ID)
                                          .ToList();

                // جلب سجلات الحضور
                var attendanceRecords = await _context.TblAttendance
                    .Include(a => a.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(a => a.Attendance_Date.Date >= startDate &&
                                a.Attendance_Date.Date <= endDate &&
                                a.Attendance_Visible == "yes" &&
                                studentIds.Contains(a.Student_ID))
                    .ToListAsync();

                var totalDays = (endDate - startDate).Days + 1;

                var result = classRooms.Select(cr =>
                {
                    var classStudents = cr.Students.Where(s => s.Student_Visible == "yes").ToList();
                    var classStudentIds = classStudents.Select(s => s.Student_ID).ToList();

                    var classRecords = attendanceRecords.Where(a => classStudentIds.Contains(a.Student_ID)).ToList();

                    var totalPresentDays = classRecords.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر");
                    var totalLateDays = classRecords.Count(r => r.Attendance_Status == "متأخر");
                    var totalAbsentDays = classRecords.Count(r => r.Attendance_Status == "غياب");

                    // نسبة انضباط الفصل = (إجمالي أيام الحضور ÷ (عدد الطلاب × إجمالي الأيام)) × 100
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

                // إضافة الترتيب
                for (int i = 0; i < result.Count; i++)
                {
                    result[i].Rank = i + 1;
                }

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
        public async Task<IActionResult> PrintMostDisciplinedClassesPdf(DateTime fromDate, DateTime toDate, int? classId)
        {
            try
            {
                var startDate = fromDate.Date;
                var endDate = toDate.Date;

                // نفس منطق جلب البيانات
                var query = _context.TblClassRoom
                    .Include(cr => cr.Class)
                    .Include(cr => cr.Students)
                    .Where(cr => cr.ClassRoom_Visible == "yes");

                if (classId.HasValue && classId.Value > 0)
                    query = query.Where(cr => cr.Class_ID == classId.Value);

                var classRooms = await query.ToListAsync();

                var classRoomIds = classRooms.Select(cr => cr.ClassRoom_ID).ToList();
                var studentIds = classRooms.SelectMany(cr => cr.Students.Where(s => s.Student_Visible == "yes"))
                                          .Select(s => s.Student_ID)
                                          .ToList();

                var attendanceRecords = await _context.TblAttendance
                    .Include(a => a.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(a => a.Attendance_Date.Date >= startDate &&
                                a.Attendance_Date.Date <= endDate &&
                                a.Attendance_Visible == "yes" &&
                                studentIds.Contains(a.Student_ID))
                    .ToListAsync();

                var totalDays = (endDate - startDate).Days + 1;

                var result = classRooms.Select(cr =>
                {
                    var classStudents = cr.Students.Where(s => s.Student_Visible == "yes").ToList();
                    var classStudentIds = classStudents.Select(s => s.Student_ID).ToList();

                    var classRecords = attendanceRecords.Where(a => classStudentIds.Contains(a.Student_ID)).ToList();

                    var totalPresentDays = classRecords.Count(r => r.Attendance_Status == "حضور" || r.Attendance_Status == "متأخر");
                    var totalLateDays = classRecords.Count(r => r.Attendance_Status == "متأخر");
                    var totalAbsentDays = classRecords.Count(r => r.Attendance_Status == "غياب");

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
                {
                    result[i].Rank = i + 1;
                }

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


      

        [HttpGet]
        public async Task<IActionResult> WeeklyLatePatternReport()
        {
            ViewBag.ReportType = "late";
            ViewBag.ReportTitle = "تقرير التأخر الأسبوعية";
            ViewBag.Classes = await _context.TblClass
                .Where(c => c.Class_Visible == "yes")
                .Select(c => new { id = c.Class_ID, name = c.Class_Name })
                .ToListAsync();

            return View("WeeklyPatternReport");
        }

        [HttpGet]
        public async Task<IActionResult> WeeklyAbsentPatternReport()
        {
            ViewBag.ReportType = "absent";
            ViewBag.ReportTitle = "تقرير الغياب الأسبوعية";
            ViewBag.Classes = await _context.TblClass
                .Where(c => c.Class_Visible == "yes")
                .Select(c => new { id = c.Class_ID, name = c.Class_Name })
                .ToListAsync();

            return View("WeeklyPatternReport");
        }

        //public async Task<IActionResult> GetWeeklyPatternReport(DateTime startDate, DateTime endDate, int? classId, int? classRoomId, string reportType = "both")
        //{
        //    try
        //    {
        //        if (startDate >= endDate)
        //        {
        //            return Json(new { success = false, message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });
        //        }

        //        var totalWeeks = (endDate - startDate).Days / 7;
        //        if (totalWeeks < 2)
        //        {
        //            return Json(new { success = false, message = "الفترة يجب أن تشمل أسبوعين على الأقل لتحليل الأنماط" });
        //        }

        //        var query = _context.TblStudent
        //            .Include(s => s.ClassRoom)
        //            .ThenInclude(cr => cr.Class)
        //            .Where(s => s.Student_Visible == "yes");

        //        if (classId.HasValue)
        //            query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

        //        if (classRoomId.HasValue)
        //            query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

        //        // ✅ تجنب تكرار الطلاب
        //        var students = await query
        //            .GroupBy(s => s.Student_ID)
        //            .Select(g => g.First())
        //            .ToListAsync();

        //        var studentIds = students.Select(s => s.Student_ID).ToList();

        //        var attendanceQuery = _context.TblAttendance
        //            .Where(a =>
        //                a.Attendance_Date.Date >= startDate.Date &&
        //                a.Attendance_Date.Date <= endDate.Date &&
        //                a.Attendance_Visible == "yes" &&
        //                studentIds.Contains(a.Student_ID)
        //            );

        //        if (reportType == "late")
        //        {
        //            attendanceQuery = attendanceQuery.Where(a => a.Attendance_Status == "متأخر");
        //        }
        //        else if (reportType == "absent")
        //        {
        //            attendanceQuery = attendanceQuery.Where(a => a.Attendance_Status == "غياب");
        //        }
        //        else
        //        {
        //            attendanceQuery = attendanceQuery.Where(a => a.Attendance_Status == "متأخر" || a.Attendance_Status == "غياب");
        //        }

        //        var attendanceRecords = await attendanceQuery
        //            .Select(a => new
        //            {
        //                a.Student_ID,
        //                a.Attendance_Date,
        //                a.Attendance_Status,
        //                DayOfWeek = a.Attendance_Date.DayOfWeek
        //            })
        //            .ToListAsync();

        //        var result = new List<StudentWeeklyPatternViewModel>();

        //        var arabicDays = new Dictionary<DayOfWeek, string>
        //{
        //    { DayOfWeek.Sunday, "الأحد" },
        //    { DayOfWeek.Monday, "الإثنين" },
        //    { DayOfWeek.Tuesday, "الثلاثاء" },
        //    { DayOfWeek.Wednesday, "الأربعاء" },
        //    { DayOfWeek.Thursday, "الخميس" },
        //    { DayOfWeek.Friday, "الجمعة" },
        //    { DayOfWeek.Saturday, "السبت" }
        //};

        //        foreach (var student in students)
        //        {
        //            var studentRecords = attendanceRecords
        //                .Where(r => r.Student_ID == student.Student_ID)
        //                .GroupBy(r => r.Attendance_Date.Date)
        //                .Select(g => g.OrderByDescending(x => x.Attendance_Date).First())
        //                .ToList();

        //            // ✅ تجاهل الطلاب بدون أي سجل
        //            if (!studentRecords.Any())
        //                continue;

        //            var dayPatterns = new List<DayPatternViewModel>();

        //            var totalOccurrences = studentRecords.Count;

        //            // تجميع الأيام الفعلية فقط
        //            var daysWithRecords = studentRecords
        //                .GroupBy(r => r.DayOfWeek)
        //                .ToDictionary(g => g.Key, g => g.ToList());

        //            foreach (var day in daysWithRecords)
        //            {
        //                var dayRecords = day.Value;
        //                if (!dayRecords.Any()) continue; // تجاهل الأيام بدون سجلات فعلية

        //                var lateCount = dayRecords.Count(r => r.Attendance_Status == "متأخر");
        //                var absentCount = dayRecords.Count(r => r.Attendance_Status == "غياب");
        //                var dayTotal = dayRecords.Count;
        //                var percentage = (double)dayTotal / totalOccurrences * 100;

        //                string patternType;
        //                if (reportType == "late")
        //                    patternType = "late";
        //                else if (reportType == "absent")
        //                    patternType = "absent";
        //                else
        //                    patternType = lateCount > 0 && absentCount > 0 ? "mixed" :
        //                                  lateCount > 0 ? "late" : "absent";

        //                dayPatterns.Add(new DayPatternViewModel
        //                {
        //                    DayName = arabicDays[day.Key],
        //                    DayNameEnglish = day.Key.ToString(),
        //                    LateCount = lateCount,
        //                    AbsentCount = absentCount,
        //                    TotalOccurrences = dayTotal,
        //                    Percentage = Math.Round(percentage, 1),
        //                    PatternType = patternType
        //                });
        //            }

        //            if (!dayPatterns.Any())
        //                continue;

        //            var mostFrequentDay = dayPatterns
        //                .OrderByDescending(d => d.TotalOccurrences)
        //                .FirstOrDefault();

        //            var totalLate = studentRecords.Count(r => r.Attendance_Status == "متأخر");
        //            var totalAbsent = studentRecords.Count(r => r.Attendance_Status == "غياب");

        //            string mostFrequentType = reportType == "late" ? "تأخر" :
        //                                      reportType == "absent" ? "غياب" :
        //                                      mostFrequentDay.PatternType == "mixed" ? "تأخر وغياب" :
        //                                      mostFrequentDay.PatternType == "late" ? "تأخر" : "غياب";

        //            result.Add(new StudentWeeklyPatternViewModel
        //            {
        //                StudentId = student.Student_ID,
        //                StudentName = student.Student_Name,
        //                StudentCode = student.Student_Code,
        //                ClassName = student.ClassRoom?.Class?.Class_Name ?? "غير محدد",
        //                ClassRoomName = student.ClassRoom?.ClassRoom_Name ?? "غير محدد",
        //                DayPatterns = dayPatterns.OrderByDescending(d => d.TotalOccurrences).ToList(),
        //                TotalLate = totalLate,
        //                TotalAbsent = totalAbsent,
        //                MostFrequentDay = mostFrequentDay.DayName,
        //                MostFrequentType = mostFrequentType,
        //                PatternStrength = (int)mostFrequentDay.Percentage
        //            });
        //        }

        //        var summary = CreateWeeklySummary(result, reportType);

        //        var viewModel = new WeeklyPatternReportViewModel
        //        {
        //            StartDate = startDate,
        //            EndDate = endDate,
        //            ClassId = classId,
        //            ClassRoomId = classRoomId,
        //            ClassName = classId.HasValue ? students.FirstOrDefault()?.ClassRoom?.Class?.Class_Name : "جميع الصفوف",
        //            ClassRoomName = classRoomId.HasValue ? students.FirstOrDefault()?.ClassRoom?.ClassRoom_Name : "جميع الفصول",
        //            Students = result.OrderByDescending(r => r.PatternStrength).ToList(),
        //            Summary = summary,
        //            ReportType = reportType
        //        };

        //        return Json(new { success = true, data = viewModel });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}


        public async Task<IActionResult> GetWeeklyPatternReportStrict(DateTime startDate, DateTime endDate, int? classId, int? classRoomId, string reportType = "both")
        {
            try
            {
                if (startDate >= endDate)
                    return Json(new { success = false, message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });

                // حساب عدد الأسابيع الكاملة
                int totalWeeks = (int)Math.Ceiling((endDate - startDate).TotalDays / 7.0);
                if (totalWeeks < 2)
                    return Json(new { success = false, message = "الفترة يجب أن تشمل أسبوعين على الأقل لتحليل الأنماط" });

                // جلب الطلاب
                var query = _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .Where(s => s.Student_Visible == "yes");

                if (classId.HasValue)
                    query = query.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue)
                    query = query.Where(s => s.ClassRoom_ID == classRoomId.Value);

                var students = await query.ToListAsync();
                var studentIds = students.Select(s => s.Student_ID).ToList();

                // جلب الحضور حسب الفترة
                var attendanceRecords = await _context.TblAttendance
                    .Where(a => a.Attendance_Date.Date >= startDate.Date &&
                                a.Attendance_Date.Date <= endDate.Date &&
                                a.Attendance_Visible == "yes" &&
                                studentIds.Contains(a.Student_ID))
                    .Select(a => new
                    {
                        a.Student_ID,
                        a.Attendance_Date,
                        a.Attendance_Status,
                        WeekNumber = EF.Functions.DateDiffWeek(startDate.Date, a.Attendance_Date.Date) // رقم الأسبوع بالنسبة للبداية
                    })
                    .ToListAsync();

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

                    // نجمع لكل يوم الأسبوع عدد الأسابيع اللي ظهر فيها الغياب/التأخر
                    var dayWeekCounts = new Dictionary<DayOfWeek, int>();

                    foreach (var record in studentRecords)
                    {
                        if (reportType == "late" && record.Attendance_Status != "متأخر") continue;
                        if (reportType == "absent" && record.Attendance_Status != "غياب") continue;
                        if (reportType == "both" && record.Attendance_Status != "غياب" && record.Attendance_Status != "متأخر") continue;

                        var day = record.Attendance_Date.DayOfWeek;
                        if (!dayWeekCounts.ContainsKey(day))
                            dayWeekCounts[day] = 0;
                        dayWeekCounts[day] = dayWeekCounts[day] + 1; // كل مرة في أسبوع مختلف نزيد
                    }

                    // احتفظ بالأيام اللي تكررت في كل أسبوع
                    var repeatedDays = dayWeekCounts.Where(d => d.Value == totalWeeks).ToList();
                    if (!repeatedDays.Any())
                        continue; // الطالب ما عندوش نمط ثابت

                    var dayPatterns = new List<DayPatternViewModel>();
                    foreach (var day in repeatedDays)
                    {
                        dayPatterns.Add(new DayPatternViewModel
                        {
                            DayName = arabicDays[day.Key],
                            DayNameEnglish = day.Key.ToString(),
                            LateCount = studentRecords.Count(r => r.Attendance_Date.DayOfWeek == day.Key && r.Attendance_Status == "متأخر"),
                            AbsentCount = studentRecords.Count(r => r.Attendance_Date.DayOfWeek == day.Key && r.Attendance_Status == "غياب"),
                            TotalOccurrences = totalWeeks,
                            Percentage = 100, // لأنه متكرر في كل أسبوع
                            PatternType = reportType == "late" ? "late" :
                                          reportType == "absent" ? "absent" :
                                          "mixed"
                        });
                    }

                    result.Add(new StudentWeeklyPatternViewModel
                    {
                        StudentId = student.Student_ID,
                        StudentName = student.Student_Name,
                        StudentCode = student.Student_Code,
                        ClassName = student.ClassRoom?.Class?.Class_Name ?? "غير محدد",
                        ClassRoomName = student.ClassRoom?.ClassRoom_Name ?? "غير محدد",
                        DayPatterns = dayPatterns,
                        TotalLate = studentRecords.Count(r => r.Attendance_Status == "متأخر"),
                        TotalAbsent = studentRecords.Count(r => r.Attendance_Status == "غياب")
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

   
    }
}


