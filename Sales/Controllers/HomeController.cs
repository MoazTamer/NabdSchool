using Dapper;
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
    public class HomeController : Controller
    {
        private static TimeZoneInfo Arabian_Standard_Time =
            TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager,
            SalesDBContext context)
        {
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                var totalStudents = await _unitOfWork.TblStudent
                    .CountAsync(s => s.Student_Visible == "yes");

                var presentCount = await _unitOfWork.TblAttendance
                    .CountAsync(a =>
                        a.Attendance_Visible == "yes" &&
                        a.Attendance_Date.Date == today &&
                        (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر")
                    );

                var lateCount = await _unitOfWork.TblAttendance
                    .CountAsync(a =>
                        a.Attendance_Visible == "yes" &&
                        a.Attendance_Date.Date == today &&
                        a.Attendance_Status == "متأخر"
                    );

                var absentCount = totalStudents - presentCount;

                var disciplinedStudentsCount = await _unitOfWork.TblStudent.Table
                    .CountAsync(s =>
                        s.Student_Visible == "yes" &&
                        !_unitOfWork.TblAttendance.Table
                            .Any(a =>
                                a.Student_ID == s.Student_ID &&
                                a.Attendance_Date.Date == today &&
                                (
                                    a.Attendance_Status == "غياب" ||
                                    a.Attendance_Status == "متأخر"
                                )
                            )
                    );



                var topStudents = await GetTopStudents(10);

                var badgeStats = await GetBadgeStatistics();

                var model = new DashboardViewModel
                {
                    TotalStudents = totalStudents,
                    Present = presentCount,
                    Late = lateCount,
                    Absent = absentCount,
                    Disciplined = disciplinedStudentsCount,
                    TopStudents = topStudents,
                    BadgeStats = badgeStats
                };

                return View(model);
            }
            catch
            {
                var model = new DashboardViewModel
                {
                    TotalStudents = 0,
                    Present = 0,
                    Absent = 0,
                    Late = 0,
                    Disciplined = 0,
                    TopStudents = new List<TopStudentBadge>(),
                    BadgeStats = new BadgeStatistics()
                };

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceRegistration()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
            var today = now.Date;

            bool dailyAbsenceExists = await _unitOfWork.TblAttendance
                .AnyAsync(a => a.Attendance_Date == today);

            if (!dailyAbsenceExists)
            {
                InsertDailyAbsence(now, today);
            }

            var totalStudents = await _unitOfWork.TblStudent.CountAsync(s => s.Student_Visible == "yes");

            var present = await _unitOfWork.TblAttendance
                .CountAsync(a => a.Attendance_Date == today &&
                            (a.Attendance_Status == "حضور" || a.Attendance_Status == "متأخر"));

            var absent = totalStudents - present;

            var model = new AttendanceViewModel
            {
                TotalStudents = totalStudents,
                Present = present,
                Absent = absent
            };

            return View(model);
        }


        public IActionResult GetTopStudentsData()
        {
            try
            {
                var topStudents = GetTopStudents(10).Result;
                var badgeStats = GetBadgeStatistics().Result;

                return Json(new
                {
                    success = true,
                    data = topStudents,
                    stats = badgeStats
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        //---------------------------------------------------------



        [HttpPost]
        public async Task<IActionResult> RegistrationAllStudents()
        {
            try
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;

                var students = _unitOfWork.TblStudent
                    .GetAll(s => s.Student_Visible == "yes")
                    .Select(s => new { s.Student_ID })
                    .ToList();

                var todaysRecords = _unitOfWork.TblAttendance
                    .GetAll(a => a.Attendance_Date.Date == today)
                    .Select(a => a.Student_ID)
                    .ToList();

                var todaysRecordsSet = new HashSet<int>(todaysRecords);

                var newAttendances = new List<TblAttendance>();
                var registeredStudents = new List<int>();

                foreach (var student in students)
                {
                    if (!todaysRecordsSet.Contains(student.Student_ID))
                    {
                        var attendance = new TblAttendance
                        {
                            Student_ID = student.Student_ID,
                            Attendance_Status = "حضور",
                            Attendance_Date = today,
                            Attendance_Time = now.TimeOfDay,
                            Attendance_Visible = "yes",
                            Attendance_AddUserID = "SYSTEM",
                            Attendance_AddDate = now
                        };

                        _unitOfWork.TblAttendance.Add(attendance);
                        registeredStudents.Add(student.Student_ID);
                    }
                }

                await _unitOfWork.Complete();

                foreach (var studentId in registeredStudents)
                {
                    try
                    {
                        await UpdateStudentPoints(studentId, today);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating points for student {studentId}: {ex.Message}");
                    }
                }

                return Json(new
                {
                    success = true,
                    message = registeredStudents.Any()
                        ? $"تم تسجيل حضور {registeredStudents.Count} طالبة بنجاح"
                        : "جميع الطالبات لديهم سجل حضور اليوم"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ: " + ex.Message
                });
            }
        }






        private async void InsertDailyAbsence(DateTime now, DateTime today)
        {
            var studentIds = _unitOfWork.TblStudent.GetAll(
                filter: s => s.Student_Visible == "yes"
            ).Select(s => s.Student_ID).ToList();

            var existingAttendance = _unitOfWork.TblAttendance.GetAll(
                filter: a => a.Attendance_Date >= today && a.Attendance_Date < today.AddDays(1)
            ).Select(a => a.Student_ID).ToList();

            var newAbsentStudents = studentIds.Except(existingAttendance).ToList();

            if (!newAbsentStudents.Any())
                return;

            var attendanceList = newAbsentStudents.Select(id => new TblAttendance
            {
                Student_ID = id,
                Attendance_Date = today,
                Attendance_Status = "غياب",
                Attendance_AddDate = now,
                Attendance_AddUserID = "SYSTEM",
                Attendance_Visible = "yes"
            }).ToList();

            await _unitOfWork.TblAttendance.AddRangeAsync(attendanceList);

            await _unitOfWork.Complete();
        }







        [HttpPost]
        public async Task<IActionResult> RegisterAttendance(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                    return Json(new { isValid = false, message = "الرجاء إدخال كود الطالبة" });

                studentCode = studentCode.Trim();

                var student = _unitOfWork.TblStudent.GetAll(
                    filter: s => s.Student_Code == studentCode && s.Student_Visible == "yes",
                    includeProperties: new[] { "ClassRoom.Class" }
                ).FirstOrDefault();

                if (student == null)
                    return Json(new { isValid = false, message = "الطالبة غير موجودة أو تم حذفها" });

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                var existingAttendance = _unitOfWork.TblAttendance.GetAll(
                    filter: a => a.Student_ID == student.Student_ID &&
                                 a.Attendance_Date == today &&
                                 a.Attendance_Visible == "yes" &&
                                 a.Attendance_Status != "غياب"
                ).OrderBy(a => a.Attendance_AddDate).LastOrDefault();

                if (existingAttendance != null)
                {
                    var excuse = new TblAttendance
                    {
                        Student_ID = student.Student_ID,
                        Attendance_Date = today,
                        Attendance_Time = currentTime,
                        Attendance_LateMinutes = 0,
                        Attendance_Status = "استئذان",
                        Attendance_Visible = "yes",
                        Attendance_AddUserID = _userManager.GetUserId(User),
                        Attendance_AddDate = now
                    };

                    _unitOfWork.TblAttendance.Add(excuse);
                    await _unitOfWork.Complete();

                    await UpdateStudentPoints(student.Student_ID, today);

                    return Json(new
                    {
                        isValid = true,
                        message = $"تم تسجيل استئذان للطالبة: {student.Student_Name}",
                        studentName = student.Student_Name,
                        className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                        status = "استئذان",
                        attendanceTime = currentTime
                    });
                }

                var attendanceTime = SchoolSettingsController.GetAttendanceTime(_unitOfWork);

                int lateMinutes = 0;
                string status = "حضور";

                if (currentTime > attendanceTime)
                {
                    lateMinutes = (int)(currentTime - attendanceTime).TotalMinutes;
                    status = "متأخر";
                }

                var attendance = new TblAttendance
                {
                    Student_ID = student.Student_ID,
                    Attendance_Date = today,
                    Attendance_Time = currentTime,
                    Attendance_LateMinutes = lateMinutes,
                    Attendance_Status = status,
                    Attendance_Visible = "yes",
                    Attendance_AddUserID = _userManager.GetUserId(User),
                    Attendance_AddDate = now
                };

                _unitOfWork.TblAttendance.Add(attendance);
                await _unitOfWork.Complete();

                await UpdateStudentPoints(student.Student_ID, today);

                var studentPoints = _unitOfWork.StudentPoints.GetAll(
                    filter: sp => sp.Student_ID == student.Student_ID
                ).FirstOrDefault();

                var newBadges = _unitOfWork.StudentBadge.GetAll(
                    filter: sb => sb.Student_ID == student.Student_ID &&
                                  sb.Badge_Visible == "yes" &&
                                  sb.Earned_Date.Date == today
                ).Join(_unitOfWork.BadgeDefinition.Table,
                       sb => new { sb.Badge_Type, sb.Badge_Level },
                       bd => new { bd.Badge_Type, bd.Badge_Level },
                       (sb, bd) => new
                       {
                           badge_name = bd.Badge_Name,
                           badge_level = bd.Badge_Level
                       }).ToList();

                string badgeMessage = "";
                if (newBadges.Any())
                    badgeMessage = $" 🏆 مبروك! حصلت على شارة جديدة: {string.Join(", ", newBadges.Select(b => $"{b.badge_name} {b.badge_level}"))}";

                return Json(new
                {
                    isValid = true,
                    message = $"تم تسجيل حضور الطالبة: {student.Student_Name}{badgeMessage}",
                    studentName = student.Student_Name,
                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    attendanceTime = currentTime.ToString(@"hh\:mm"),
                    lateMinutes = lateMinutes,
                    status = status == "متأخر" ? "متأخر" : "حاضر",
                    totalPoints = studentPoints?.Total_Points ?? 0,
                    attendanceStreak = studentPoints?.Attendance_Streak ?? 0,
                    newBadges = newBadges
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isValid = false,
                    message = "خطأ في تسجيل الحضور: " + ex.Message
                });
            }
        }







        [HttpPost]
        public async Task<IActionResult> RegisterExcuse(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                    return Json(new { isValid = false, message = "الرجاء إدخال كود الطالبة" });

                studentCode = studentCode.Trim();

                var student = _unitOfWork.TblStudent.GetAll(
                    filter: s => s.Student_Code == studentCode && s.Student_Visible == "yes",
                    includeProperties: new[] { "ClassRoom.Class" }
                ).FirstOrDefault();

                if (student == null)
                    return Json(new { isValid = false, message = "الطالبة غير موجودة أو تم حذفها" });

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Arabian_Standard_Time);
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                var existingExcuse = _unitOfWork.TblAttendance.GetAll(
                    filter: a => a.Student_ID == student.Student_ID &&
                                 a.Attendance_Date == today &&
                                 a.Attendance_Status == "استئذان" &&
                                 a.Attendance_Visible == "yes"
                ).FirstOrDefault();

                if (existingExcuse != null)
                    return Json(new { isValid = false, message = "تم تسجيل استئذان للطالبة مسبقاً اليوم" });

                var excuse = new TblAttendance
                {
                    Student_ID = student.Student_ID,
                    Attendance_Date = today,
                    Attendance_Time = currentTime,
                    Attendance_LateMinutes = 0,
                    Attendance_Status = "استئذان",
                    Attendance_Visible = "yes",
                    Attendance_AddUserID = _userManager.GetUserId(User),
                    Attendance_AddDate = now
                };

                _unitOfWork.TblAttendance.Add(excuse);
                await _unitOfWork.Complete();

                await UpdateStudentPoints(student.Student_ID, today);

                return Json(new
                {
                    isValid = true,
                    message = $"تم تسجيل استئذان للطالبة: {student.Student_Name}",
                    studentName = student.Student_Name,
                    className = $"{student.ClassRoom?.Class?.Class_Name} - {student.ClassRoom?.ClassRoom_Name}",
                    status = "استئذان",
                    attendanceTime = currentTime,
                });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, message = "خطأ في تسجيل الاستئذان: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/Login");
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Authorized()
        {
            return View();
        }


        private async Task<int> CalculateStudentPoints(int studentId, DateTime date)
        {
            var lastAttendance = _unitOfWork.TblAttendance.GetAll(
                filter: a => a.Student_ID == studentId &&
                             a.Attendance_Date.Date == date.Date &&
                             a.Attendance_Visible == "yes",
                orderBy: a => a.Attendance_ID,
                orderByDirection: OrderBy.Descending
            ).FirstOrDefault();

            if (lastAttendance == null) return 0;

            return lastAttendance.Attendance_Status switch
            {
                "حضور" => 10,
                "متأخر" => 3,
                "غياب" => -5,
                "استئذان" => 0,
                _ => 0
            };
        }

        private async Task UpdateStudentPoints(int studentId, DateTime date)
        {
            var points = await CalculateStudentPoints(studentId, date);

            var studentPoints = _unitOfWork.StudentPoints.GetAll(
                filter: sp => sp.Student_ID == studentId
            ).FirstOrDefault();

            if (studentPoints == null)
            {
                studentPoints = new StudentPoints
                {
                    Student_ID = studentId,
                    Total_Points = points,
                    Monthly_Points = points,
                    Attendance_Streak = points > 0 ? 1 : 0,
                    Last_Updated = date
                };
                _unitOfWork.StudentPoints.Add(studentPoints);
            }
            else
            {
                // تحديث النقاط
                studentPoints.Total_Points += points;
                studentPoints.Monthly_Points += points;

                if (points > 0)
                {
                    var lastDate = studentPoints.Last_Updated.Date;
                    if (date.Date == lastDate.AddDays(1))
                    {
                        studentPoints.Attendance_Streak++;
                    }
                    else if (date.Date > lastDate.AddDays(1))
                    {
                        studentPoints.Attendance_Streak = 1;
                    }
                }
                else
                {
                    studentPoints.Attendance_Streak = 0;
                }

                studentPoints.Last_Updated = date;
            }

            await _unitOfWork.Complete();

            await CheckAndAwardBadges(studentId);
        }

        private async Task CheckAndAwardBadges(int studentId)
        {
            var studentPoints = _unitOfWork.StudentPoints.GetAll(
                sp => sp.Student_ID == studentId
            ).FirstOrDefault();

            if (studentPoints == null) return;

            var existingBadges = _unitOfWork.StudentBadge.GetAll(
                sb => sb.Student_ID == studentId && sb.Badge_Visible == "yes"
            ).Select(sb => new { sb.Badge_Type, sb.Badge_Level }).ToList();

            var badgeDefinitions = _unitOfWork.BadgeDefinition.GetAll(
                bd => bd.Is_Active,
                orderBy: bd => bd.Required_Points
            ).ToList();

            foreach (var definition in badgeDefinitions)
            {
                if (existingBadges.Any(eb => eb.Badge_Type == definition.Badge_Type &&
                                              eb.Badge_Level == definition.Badge_Level))
                    continue;

                bool shouldAward = false;

                if (definition.Badge_Type == "انضباط" &&
                    studentPoints.Total_Points >= definition.Required_Points)
                {
                    shouldAward = true;
                }

                if (definition.Badge_Type == "حضور_متتالي" &&
                    studentPoints.Attendance_Streak >= (definition.Required_Points / 10))
                {
                    shouldAward = true;
                }

                if (shouldAward)
                {
                    var badge = new StudentBadge
                    {
                        Student_ID = studentId,
                        Badge_Type = definition.Badge_Type,
                        Badge_Level = definition.Badge_Level,
                        Points = definition.Required_Points,
                        Earned_Date = DateTime.Now,
                        Badge_Visible = "yes"
                    };

                    _unitOfWork.StudentBadge.Add(badge);
                }
            }

            await _unitOfWork.Complete();
        }


        private async Task<List<TopStudentBadge>> GetTopStudents(int count = 10)
        {
            var topStudents = _unitOfWork.StudentPoints.GetAll(
                filter: sp => sp.Student.Student_Visible == "yes",
                includeProperties: new[] { "Student" },
                orderBy: sp => sp.Total_Points,
                orderByDirection: OrderBy.Descending
            ).Take(count).ToList();

            var result = new List<TopStudentBadge>();

            foreach (var studentPoints in topStudents)
            {
                var badges = _unitOfWork.StudentBadge.GetAll(
                    sb => sb.Student_ID == studentPoints.Student_ID && sb.Badge_Visible == "yes"
                ).Join(
                    _unitOfWork.BadgeDefinition.GetAll(bd => bd.Is_Active),
                    sb => new { sb.Badge_Type, sb.Badge_Level },
                    bd => new { bd.Badge_Type, bd.Badge_Level },
                    (sb, bd) => new StudentBadgeInfo
                    {
                        Badge_Name = bd.Badge_Name,
                        Badge_Level = bd.Badge_Level,
                        Badge_Icon = bd.Badge_Icon,
                        Badge_Color = bd.Badge_Color,
                        Points = sb.Points
                    }
                ).ToList();

                var highestBadge = badges.OrderByDescending(b => b.Points).FirstOrDefault();

                result.Add(new TopStudentBadge
                {
                    Student_ID = studentPoints.Student_ID,
                    Student_Name = studentPoints.Student.Student_Name,
                    Total_Points = studentPoints.Total_Points,
                    Attendance_Streak = studentPoints.Attendance_Streak,
                    Badges = badges,
                    HighestBadgeLevel = highestBadge?.Badge_Level ?? "لا يوجد",
                    BadgeColor = highestBadge?.Badge_Color ?? "#6c757d"
                });
            }

            return result;
        }

        private async Task<BadgeStatistics> GetBadgeStatistics()
        {
            var allBadges = _unitOfWork.StudentBadge.GetAll(
                filter: sb => sb.Badge_Visible == "yes"
            ).ToList();

            return new BadgeStatistics
            {
                TotalBadgesEarned = allBadges.Count,
                DiamondBadges = allBadges.Count(b => b.Badge_Level == "ماسي"),
                GoldBadges = allBadges.Count(b => b.Badge_Level == "ذهبي"),
                SilverBadges = allBadges.Count(b => b.Badge_Level == "فضي"),
                BronzeBadges = allBadges.Count(b => b.Badge_Level == "برونزي")
            };
        }

        [HttpGet]
        public IActionResult GetStudentBadges(int studentId)
        {
            try
            {
                var studentPoints = _unitOfWork.StudentPoints.GetAll(
                    filter: sp => sp.Student_ID == studentId,
                    includeProperties: new[] { "Student" }
                ).FirstOrDefault();

                var badges = (from sb in _unitOfWork.StudentBadge.GetAll(
                                  filter: sb => sb.Student_ID == studentId && sb.Badge_Visible == "yes"
                              )
                              join bd in _unitOfWork.BadgeDefinition.GetAll(filter: bd => bd.Is_Active)
                              on new { sb.Badge_Type, sb.Badge_Level } equals new { bd.Badge_Type, bd.Badge_Level }
                              select new
                              {
                                  badge_name = bd.Badge_Name,
                                  badge_level = bd.Badge_Level,
                                  badge_icon = bd.Badge_Icon,
                                  badge_color = bd.Badge_Color,
                                  points = sb.Points,
                                  earned_date = sb.Earned_Date,
                                  description = bd.Description
                              }).ToList();

                return Json(new
                {
                    success = true,
                    student_name = studentPoints?.Student.Student_Name ?? "",
                    total_points = studentPoints?.Total_Points ?? 0,
                    monthly_points = studentPoints?.Monthly_Points ?? 0,
                    attendance_streak = studentPoints?.Attendance_Streak ?? 0,
                    badges = badges
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ResetMonthlyPoints()
        {
            try
            {
                var allPoints = _unitOfWork.StudentPoints.GetAll().ToList();

                foreach (var point in allPoints)
                {
                    point.Monthly_Points = 0;
                }

                _unitOfWork.Complete();

                return Json(new { success = true, message = "تم إعادة تعيين النقاط الشهرية بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

    }
}
