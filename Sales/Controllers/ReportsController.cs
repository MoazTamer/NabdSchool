using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                        int consecutive = 0;
                        var current = reportDate.Date;

                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        // count consecutive days: only count when there is a record AND status == "غياب"
                        for (int i = 0; i < 30; i++)
                        {
                            if (dateStatusMap != null && dateStatusMap.TryGetValue(current, out var statusOnDay))
                            {
                                // there is a record for this date
                                if (string.Equals(statusOnDay, "غياب", StringComparison.OrdinalIgnoreCase))
                                {
                                    consecutive++;
                                    current = current.AddDays(-1);
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        absentList.Add(new AbsentStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = consecutive,
                            Notes = consecutive >= 3 ? "تحذير: غياب متكرر" : ""
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
                        // نحسب الأيام المتتالية للغياب: نتحقق يومًا يومًا للخلف حتى نجد يوم فيه سجل مختلف عن "غياب" أو لا يوجد سجل
                        int consecutive = 0;
                        var current = reportDate;

                        studentDateStatus.TryGetValue(sid, out var dateStatusMap); // قد يكون null

                        for (int i = 0; i < 30; i++)
                        {
                            if (dateStatusMap != null && dateStatusMap.TryGetValue(current, out var statusOnDay))
                            {
                                // if status exists and is 'غياب' -> count it and move to previous day
                                if (string.Equals(statusOnDay, "غياب", StringComparison.OrdinalIgnoreCase))
                                {
                                    consecutive++;
                                    current = current.AddDays(-1);
                                    continue;
                                }
                                else
                                {
                                    // found a presence (حضور/متأخر/غيره) -> stop counting
                                    break;
                                }
                            }
                            else
                            {
                                // no record for this date => stop counting (we treat absence sequence as broken)
                                break;
                            }
                        }

                        var st = activeStudents.First(s => s.Student_ID == sid);

                        absentStudentsList.Add(new AbsentStudentViewModel
                        {
                            StudentId = st.Student_ID,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = consecutive,
                            Notes = consecutive >= 3 ? "تحذير: غياب متكرر" : ""
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
                        int consecutive = 0;
                        var current = reportDate.Date;

                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        // count consecutive late days
                        for (int i = 0; i < 30; i++)
                        {
                            if (dateStatusMap != null && dateStatusMap.TryGetValue(current, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    consecutive++;
                                    current = current.AddDays(-1);
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
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
                            ConsecutiveAbsenceDays = consecutive,
                            Notes = consecutive >= 3 ? "تحذير: تأخر متكرر" : ""
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
                        int consecutive = 0;
                        var current = reportDate;

                        studentDateStatus.TryGetValue(sid, out var dateStatusMap);

                        // حساب الأيام المتتالية للتأخر
                        for (int i = 0; i < 30; i++)
                        {
                            if (dateStatusMap != null && dateStatusMap.TryGetValue(current, out var statusOnDay))
                            {
                                if (string.Equals(statusOnDay, "متأخر", StringComparison.OrdinalIgnoreCase))
                                {
                                    consecutive++;
                                    current = current.AddDays(-1);
                                    continue;
                                }
                                else break;
                            }
                            else break;
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
                            ConsecutiveAbsenceDays = consecutive,
                            Notes = consecutive >= 3 ? "تحذير: تأخر متكرر" : ""
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

        public IActionResult StudentReport(string studentCode, DateTime? fromDate, DateTime? date)
        {
            ViewBag.StudentCode = studentCode;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            ViewBag.Date = date?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetStudentAttendanceReport(string studentCode, DateTime? date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);
                //var minDate = reportDate.AddDays(-30);

                var code = studentCode.Trim();

                var student = await _context.TblStudent
                .Include(s => s.ClassRoom)
                .ThenInclude(cr => cr.Class)
                .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim() && s.Student_Visible == "yes");

                

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

                // تجهيز جدول الأيام 30 يوم
                var result = new List<StudentDayStatusViewModel>();

                for (int i = 0; i <= 30; i++)
                {
                    var day = reportDate.AddDays(-i).Date;

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
                int present = result.Count(r => r.Status == "حضور");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                // حساب التأخر المتتالي
                int consecutiveLate = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر") consecutiveLate++;
                    else break;
                }

                // حساب الغياب المتتالي
                int consecutiveAbsent = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب") consecutiveAbsent++;
                    else break;
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
                    ConsecutiveLate = consecutiveLate,
                    ConsecutiveAbsent = consecutiveAbsent
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrintStudentAttendancePdf(string studentCode, DateTime date, DateTime? fromDate)
        {
            try
            {
                var reportDate = date.Date;
                var minDate = fromDate?.Date ?? reportDate.AddDays(-30);
                //var minDate = reportDate.AddDays(-30);

                var student = await _context.TblStudent
                    .Include(s => s.ClassRoom)
                    .ThenInclude(cr => cr.Class)
                    .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim() && s.Student_Visible == "yes");

                if (student == null)
                {
                    return Json(new { success = false, message = "الطالبة غير موجودة" });
                }

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

                // تجهيز جدول الأيام 30 يوم
                var result = new List<StudentDayStatusViewModel>();

                for (int i = 0; i <= 30; i++)
                {
                    var day = reportDate.AddDays(-i).Date;

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
                int present = result.Count(r => r.Status == "حضور");
                int late = result.Count(r => r.Status == "متأخر");
                int absent = result.Count(r => r.Status == "غياب");

                // حساب التأخر المتتالي
                int consecutiveLate = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "متأخر") consecutiveLate++;
                    else break;
                }

                // حساب الغياب المتتالي
                int consecutiveAbsent = 0;
                foreach (var row in result.OrderByDescending(r => r.Date))
                {
                    if (row.Status == "غياب") consecutiveAbsent++;
                    else break;
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
                    ConsecutiveLate = consecutiveLate,
                    ConsecutiveAbsent = consecutiveAbsent
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


