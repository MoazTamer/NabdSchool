using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;
using SalesRepository.Repository;

namespace Sales.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        string Title = "الطلاب";

        public StudentController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        //[Authorize(Policy = "Student_View")]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                ViewBag.Classes = classes.Select(c => new SelectListItem
                {
                    Value = c.Class_ID.ToString(),
                    Text = c.Class_Name
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult GetStudents(int? classId, int? classRoomId)
        {
            try
            {
                var studentsQuery = _unitOfWork.TblStudent.GetAll(
                    obj => obj.Student_Visible == "yes",
                    new[] { "ClassRoom", "ClassRoom.Class" }
                );

                if (classId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom.Class_ID == classId.Value);
                }

                if (classRoomId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom_ID == classRoomId.Value);
                }

                var data = studentsQuery.Select(s => new
                {
                    student_ID = s.Student_ID,
                    student_Name = s.Student_Name,
                    student_Code = s.Student_Code,
                    student_Phone = s.Student_Phone,
                    student_Gender = s.Student_Gender,
                    className = s.ClassRoom?.Class?.Class_Name ?? "",
                    classRoomName = s.ClassRoom?.ClassRoom_Name ?? "",
                    classRoom_ID = s.ClassRoom_ID
                }).ToList();

                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetClassRoomsByClass(int classId)
        {
            try
            {
                var classRooms = _unitOfWork.TblClassRoom.GetAll(
                    obj => obj.Class_ID == classId && obj.ClassRoom_Visible == "yes"
                ).Select(cr => new SelectListItem
                {
                    Value = cr.ClassRoom_ID.ToString(),
                    Text = cr.ClassRoom_Name
                }).ToList();

                return Json(new { data = classRooms });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }


        #region Student CRUD

        [Authorize (Policy = "Student_Create1")]
        [HttpGet]
        public IActionResult CreateStudent()
        {
            try
            {
                var model = new ModelStudent();

                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                ViewBag.Classes = classes.Select(c => new SelectListItem
                {
                    Value = c.Class_ID.ToString(),
                    Text = c.Class_Name
                }).ToList();

                return PartialView("_StudentCreate", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Error: {ex.Message}<br>StackTrace: {ex.StackTrace}</div>");
            }
        }

        [Authorize(Policy = "Student_Create1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(ModelStudent model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Student_Code))
                {
                    var checkStudent = _unitOfWork.TblStudent.GetFirstOrDefault(
                        obj => obj.Student_Code == model.Student_Code.Trim() && obj.Student_Visible == "yes"
                    );
                    if (checkStudent != null)
                    {
                        return Json(new { isValid = false, title = Title, message = "كود الطالبة موجودة بالفعل" });
                    }
                }

                var student = new TblStudent
                {
                    ClassRoom_ID = model.ClassRoom_ID,
                    Student_Name = model.Student_Name.Trim(),
                    Student_Code = model.Student_Code?.Trim(),
                    Student_Phone = model.Student_Phone?.Trim(),
                    Student_Address = model.Student_Address?.Trim(),
                    Student_BirthDate = model.Student_BirthDate,
                    Student_Gender = model.Student_Gender,
                    Student_Notes = model.Student_Notes?.Trim(),
                    Student_Visible = "yes",
                    Student_AddUserID = _userManager.GetUserId(User),
                    Student_AddDate = DateTime.Now
                };

                _unitOfWork.TblStudent.Add(student);

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [Authorize (Policy = "Student_Edit1")]
        [HttpGet]
        public IActionResult EditStudent(int id)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetFirstOrDefault(
                    obj => obj.Student_ID == id,
                    new[] { "ClassRoom", "ClassRoom.Class" }
                );

                if (student == null)
                {
                    return Content("<div class='alert alert-danger'>الطالب غير موجود</div>");
                }

                var model = new ModelStudent
                {
                    Student_ID = student.Student_ID,
                    ClassRoom_ID = student.ClassRoom_ID,
                    Student_Name = student.Student_Name,
                    Student_Code = student.Student_Code,
                    Student_Phone = student.Student_Phone,
                    Student_Address = student.Student_Address,
                    Student_BirthDate = student.Student_BirthDate,
                    Student_Gender = student.Student_Gender,
                    Student_Notes = student.Student_Notes
                };

                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                ViewBag.Classes = classes.Select(c => new SelectListItem
                {
                    Value = c.Class_ID.ToString(),
                    Text = c.Class_Name,
                    Selected = c.Class_ID == student.ClassRoom?.Class_ID
                }).ToList();

                if (student.ClassRoom?.Class_ID != null)
                {
                    var classRooms = _unitOfWork.TblClassRoom.GetAll(
                        obj => obj.Class_ID == student.ClassRoom.Class_ID && obj.ClassRoom_Visible == "yes"
                    ).ToList();

                    ViewBag.ClassRooms = classRooms.Select(cr => new SelectListItem
                    {
                        Value = cr.ClassRoom_ID.ToString(),
                        Text = cr.ClassRoom_Name,
                        Selected = cr.ClassRoom_ID == student.ClassRoom_ID
                    }).ToList();
                }

                return PartialView("_StudentEdit", model);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Error: {ex.Message}</div>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(ModelStudent model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Student_Code))
                {
                    var checkStudent = _unitOfWork.TblStudent.GetFirstOrDefault(
                        obj => obj.Student_ID != model.Student_ID &&
                               obj.Student_Code == model.Student_Code.Trim() &&
                               obj.Student_Visible == "yes"
                    );
                    if (checkStudent != null)
                    {
                        return Json(new { isValid = false, title = Title, message = "رقم الطالب موجود بالفعل" });
                    }
                }

                var student = _unitOfWork.TblStudent.GetById(model.Student_ID);
                student.ClassRoom_ID = model.ClassRoom_ID;
                student.Student_Name = model.Student_Name.Trim();
                student.Student_Code = model.Student_Code?.Trim();
                student.Student_Phone = model.Student_Phone?.Trim();
                student.Student_Address = model.Student_Address?.Trim();
                student.Student_BirthDate = model.Student_BirthDate;
                student.Student_Gender = model.Student_Gender;
                student.Student_Notes = model.Student_Notes?.Trim();
                student.Student_EditUserID = _userManager.GetUserId(User);
                student.Student_EditDate = DateTime.Now;

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [Authorize (Policy = "Student_Delete1")]
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetById(id);

                if (student == null)
                {
                    return Json(new { isValid = false, title = Title, message = "الطالب غير موجود" });
                }

                student.Student_Visible = "no";
                student.Student_DeleteUserID = _userManager.GetUserId(User);
                student.Student_DeleteDate = DateTime.Now;

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "عفوا ، لم يتم الحذف" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحذف بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        #endregion

        #region Import from Excel

        [Authorize (Policy = "Student_Create1")]
        [HttpGet]
        public IActionResult ImportStudents()
        {
            try
            {
                return PartialView("_StudentImport");
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Error: {ex.Message}<br>StackTrace: {ex.StackTrace}</div>");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStudents(IFormFile excelFile)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك اختر ملف Excel" });
                }

                var successCount = 0;
                var errorCount = 0;
                var errors = new List<string>();
                var userId = _userManager.GetUserId(User);

                var classes = _unitOfWork.TblClass.GetAll(c => c.Class_Visible == "yes").ToList();
                var classrooms = _unitOfWork.TblClassRoom.GetAll(c => c.ClassRoom_Visible == "yes").ToList();
                var students = _unitOfWork.TblStudent.GetAll(s => s.Student_Visible == "yes").ToList();

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var studentCode = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                var studentName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                var classNumber = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                var classRoomNumber = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                var phone = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentCode))
                                {
                                    errors.Add($"الصف {row}: اسم الطالب أو رقم الطالب مفقود");
                                    errorCount++;
                                    continue;
                                }

                                if (students.Any(s => s.Student_Code == studentCode))
                                {
                                    errors.Add($"الصف {row}: الطالب {studentName} موجود بالفعل برقم {studentCode}");
                                    errorCount++;
                                    continue;
                                }

                          
                                TblClass classEntity = null;

                                if (!string.IsNullOrEmpty(classNumber))
                                {
                                    classEntity = classes.FirstOrDefault(c => c.Class_Name == classNumber);

                                    if (classEntity == null)
                                    {
                                        classEntity = new TblClass
                                        {
                                            Class_Name = classNumber,
                                            Class_Visible = "yes",
                                            Class_AddUserID = userId,
                                            Class_AddDate = DateTime.Now
                                        };

                                        _unitOfWork.TblClass.Add(classEntity);
                                        classes.Add(classEntity);
                                    }
                                }

                                int? classRoomId = null;

                                if (!string.IsNullOrEmpty(classRoomNumber))
                                {
                                    var classRoom = classrooms.FirstOrDefault(c => c.ClassRoom_Name == classRoomNumber);

                                    if (classRoom == null)
                                    {
                                        if (classEntity == null)
                                        {
                                            errors.Add($"الصف {row}: لم يتم العثور على الصف {classNumber}");
                                            errorCount++;
                                            continue;
                                        }

                                        classRoom = new TblClassRoom
                                        {
                                            Class_ID = classEntity.Class_ID,
                                            ClassRoom_Name = classRoomNumber,
                                            ClassRoom_Visible = "yes",
                                            ClassRoom_AddUserID = userId,
                                            ClassRoom_AddDate = DateTime.Now
                                        };

                                        _unitOfWork.TblClassRoom.Add(classRoom);
                                        classrooms.Add(classRoom);
                                    }

                                    classRoomId = classRoom.ClassRoom_ID;
                                }

                                if (!classRoomId.HasValue)
                                {
                                    errors.Add($"الصف {row}: لم يتم العثور على الفصل للطالب {studentName}");
                                    errorCount++;
                                    continue;
                                }

                                var student = new TblStudent
                                {
                                    ClassRoom_ID = classRoomId.Value,
                                    Student_Name = studentName,
                                    Student_Code = studentCode,
                                    Student_Phone = phone,
                                    Student_Visible = "yes",
                                    Student_AddUserID = userId,
                                    Student_AddDate = DateTime.Now
                                };

                                _unitOfWork.TblStudent.Add(student);
                                students.Add(student);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"الصف {row}: {ex.Message}");
                                errorCount++;
                            }
                        }
                    }
                }

                await _unitOfWork.Complete();

                var message = $"تم استيراد {successCount} طالب بنجاح";
                if (errorCount > 0)
                    message += $" | فشل استيراد {errorCount} طالب";

                return Json(new
                {
                    isValid = true,
                    title = Title,
                    message = message,
                    successCount = successCount,
                    errorCount = errorCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "خطأ: " + ex.Message });
            }
        }

        #endregion

        #region Archive (Deleted Students)

        [HttpGet]
        public IActionResult Archive()
        {
            try
            {
                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                ViewBag.Classes = classes.Select(c => new SelectListItem
                {
                    Value = c.Class_ID.ToString(),
                    Text = c.Class_Name
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDeletedStudents(int? classId, int? classRoomId)
        {
            try
            {
                var studentsQuery = _unitOfWork.TblStudent
                    .GetAll(
                        s => s.Student_Visible == "no",
                        new[] { "ClassRoom", "ClassRoom.Class" }
                    );

                if (classId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom_ID == classRoomId.Value);

                var studentsList = studentsQuery.ToList();

                var deletedUserIds = studentsList
                    .Where(s => !string.IsNullOrEmpty(s.Student_DeleteUserID))
                    .Select(s => s.Student_DeleteUserID)
                    .Distinct()
                    .ToList();

                var users = await _userManager.Users
                    .Where(u => deletedUserIds.Contains(u.Id))
                    .ToListAsync();

                var userDict = users.ToDictionary(u => u.Id, u => u.UserName);

                var data = studentsList.Select(s => new
                {
                    student_ID = s.Student_ID,
                    student_Name = s.Student_Name,
                    student_Code = s.Student_Code,
                    student_Phone = s.Student_Phone,
                    student_Gender = s.Student_Gender,
                    className = s.ClassRoom?.Class?.Class_Name ?? "",
                    classRoomName = s.ClassRoom?.ClassRoom_Name ?? "",
                    deletedBy = s.Student_DeleteUserID != null && userDict.ContainsKey(s.Student_DeleteUserID)
                                ? userDict[s.Student_DeleteUserID]
                                : "غير معروف",
                    deletedDate = s.Student_DeleteDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
                    classRoom_ID = s.ClassRoom_ID
                }).ToList();

                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> RestoreStudent(int id)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetById(id);

                if (student == null)
                {
                    return Json(new { isValid = false, title = Title, message = "الطالب غير موجود" });
                }

                student.Student_Visible = "yes";
                student.Student_EditUserID = _userManager.GetUserId(User);
                student.Student_EditDate = DateTime.Now;
            

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "عفوا ، لم يتم الاستعادة" });
                }

                return Json(new { isValid = true, title = Title, message = "تم استعادة الطالب بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PermanentDeleteStudent(int id)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetById(id);

                if (student == null)
                {
                    return Json(new { isValid = false, title = Title, message = "الطالب غير موجود" });
                }

                _unitOfWork.TblStudent.DeleteByEntity(student);

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "عفوا ، لم يتم الحذف" });
                }

                return Json(new { isValid = true, title = Title, message = "تم حذف الطالب نهائياً" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        #endregion


        #region Show QR Code for Student
        public IActionResult ShowQRCode(int id)
        {
            try
            {
                var student = _unitOfWork.TblStudent.GetFirstOrDefault(
                    obj => obj.Student_ID == id && obj.Student_Visible == "yes"
                );
                if (student == null)
                {
                    return Json(new { error = true, message = "الطالب غير موجود" });
                }
                return View(student);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }
        #endregion


        #region Print Student Cards

        [HttpGet]
        public IActionResult PrintStudentCards(int? classId, int? classRoomId)
        {
            try
            {
                var studentsQuery = _unitOfWork.TblStudent.GetAll(
                    s => s.Student_Visible == "yes",
                    new[] { "ClassRoom", "ClassRoom.Class" }
                );

                if (classId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom.Class_ID == classId.Value);

                if (classRoomId.HasValue)
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom_ID == classRoomId.Value);

                var students = studentsQuery
                    .Select(s => new StudentCardViewModel
                    {
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        StudentPhone = s.Student_Phone,
                        ClassName = s.ClassRoom.Class.Class_Name ?? "",
                        ClassRoomName = s.ClassRoom.ClassRoom_Name ?? ""
                    })
                    .ToList();

                if (!students.Any())
                    return Content("لا يوجد طلاب للطباعة");

                var pdfService = new ReportPdfService();
                byte[] pdfBytes = pdfService.GenerateStudentCards(students);

                return File(pdfBytes, "application/pdf", $"بطاقات_الطلاب_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return Content($"خطأ: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult PrintSingleStudentCard(int studentId)
        {
            try
            {
                var student = _unitOfWork.TblStudent
                    .GetAll(
                        s => s.Student_ID == studentId && s.Student_Visible == "yes",
                        new[] { "ClassRoom", "ClassRoom.Class" }
                    )
                    .Select(s => new StudentCardViewModel
                    {
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        StudentPhone = s.Student_Phone,
                        ClassName = s.ClassRoom.Class.Class_Name ?? "",
                        ClassRoomName = s.ClassRoom.ClassRoom_Name ?? ""
                    })
                    .FirstOrDefault();

                if (student == null)
                    return Content("الطالب غير موجود");

                var pdfService = new ReportPdfService();
                byte[] pdfBytes = pdfService.GenerateStudentCards(
                    new List<StudentCardViewModel> { student }
                );

                return File(pdfBytes, "application/pdf", $"بطاقة_{student.StudentName}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return Content($"خطأ: {ex.Message}");
            }
        }

        #endregion
    }
}