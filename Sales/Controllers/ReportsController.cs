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

        // 
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

                // 2) Today's attendance (present/late) set
                var todayAttendance = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate.Date &&
                        a.Attendance_Visible == "yes")
                    .Select(a => new { a.Student_ID, a.Attendance_Status })
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

                // الطلاب المتأخرين اليوم
                var lateTodayIds = todayAttendance
                    .Where(x => x.Attendance_Status == "متأخر")
                    .Select(x => x.Student_ID)
                    .ToList();

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

                        lateList.Add(new AbsentStudentViewModel
                        {
                            StudentId = sid,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
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

                // جلب سجلات الحضور لليوم المطلوب (حضور/متأخر) لتحديد المتأخرين
                var todayRecords = await _context.TblAttendance
                    .Where(a =>
                        a.Attendance_Date.Date == reportDate &&
                        a.Attendance_Visible == "yes" &&
                        relevantStudentIds.Contains(a.Student_ID))
                    .Select(a => new { a.Student_ID, a.Attendance_Status })
                    .ToListAsync();

                // الطلاب المتأخرين اليوم
                var lateTodayIds = todayRecords
                    .Where(x => x.Attendance_Status == "متأخر")
                    .Select(x => x.Student_ID)
                    .ToList();

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

                        lateList.Add(new AbsentStudentViewModel
                        {
                            StudentId = st.Student_ID,
                            StudentName = st.Student_Name,
                            StudentCode = st.Student_Code,
                            StudentPhone = st.Student_Phone,
                            ConsecutiveAbsenceDays = consecutive, // ممكن تسميها ConsecutiveLateDays
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

        //
        public IActionResult StudentReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentAttendanceReport(string studentCode, DateTime? date)
        {
            try
            {
                var reportDate = date?.Date ?? DateTime.Today;
                var minDate = reportDate.AddDays(-30);

                // جلب بيانات الطالب
                //var student = await _context.TblStudent
                //    .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim() && s.Student_Visible == "yes");
                var code = studentCode.Trim();

                var student = await _context.TblStudent
                .Include(s => s.ClassRoom)
                .ThenInclude(cr => cr.Class)
                .FirstOrDefaultAsync(s => s.Student_Code == studentCode.Trim() && s.Student_Visible == "yes");

                

                if (student == null)
                    return Json(new { success = false, message = "الطالب غير موجود" });

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

                    result.Add(new StudentDayStatusViewModel
                    {
                        Date = day,
                        Status = record?.Attendance_Status ?? "غياب",
                        Time = record?.Attendance_Time.ToString(@"hh\:mm") ?? "",
                        Notes = ""
                    });
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
                    Days = result.OrderBy(r => r.Date).ToList(),
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

        //[HttpGet]
        //public async Task<IActionResult> PrintStudentAttendancePdf(int studentId, DateTime date)
        //{
        //    try
        //    {
        //        // استدعاء نفس API الخاصة بالتقرير
        //        var result = await GetStudentAttendanceReport(studentId, date) as JsonResult;
        //        dynamic data = result.Value;

        //        if (data.success == false)
        //            return Content("Error: " + data.message);

        //        var model = Newtonsoft.Json.JsonConvert
        //            .DeserializeObject<StudentAttendanceReportViewModel>(data.data.ToString());

        //        // إنشاء PDF
        //        var pdf = _pdfService.GenerateStudentReport(model);

        //        return File(pdf, "application/pdf", $"تقرير_{model.StudentName}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("PDF Error: " + ex.Message);
        //    }
        //}

    }
}


