using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                // Get all classes for filter dropdown
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

                // Get all classes
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
                // Check if student code already exists
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

                // Get all classes
                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                ViewBag.Classes = classes.Select(c => new SelectListItem
                {
                    Value = c.Class_ID.ToString(),
                    Text = c.Class_Name,
                    Selected = c.Class_ID == student.ClassRoom?.Class_ID
                }).ToList();

                // Get classrooms for selected class
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
                // Check if student code already exists (excluding current student)
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

                // Soft delete
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
        [Authorize]
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

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        // Start from row 2 (skip header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Read data from Excel
                                var studentCode = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                var studentName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                var classRoomNumber = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                var classNumber = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                var phone = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                                // Validate required fields
                                if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentCode))
                                {
                                    errors.Add($"الصف {row}: اسم الطالب أو رقم الطالب مفقود");
                                    errorCount++;
                                    continue;
                                }

                                // Check if student already exists
                                var existingStudent = _unitOfWork.TblStudent.GetFirstOrDefault(
                                    obj => obj.Student_Code == studentCode && obj.Student_Visible == "yes"
                                );

                                if (existingStudent != null)
                                {
                                    errors.Add($"الصف {row}: الطالب {studentName} موجود بالفعل برقم {studentCode}");
                                    errorCount++;
                                    continue;
                                }

                                // Find or create classroom
                                int? classRoomId = null;

                                if (!string.IsNullOrEmpty(classRoomNumber))
                                {
                                    var classRoom = _unitOfWork.TblClassRoom.GetFirstOrDefault(
                                        obj => obj.ClassRoom_Name == classRoomNumber && obj.ClassRoom_Visible == "yes"
                                    );

                                    if (classRoom != null)
                                    {
                                        classRoomId = classRoom.ClassRoom_ID;
                                    }
                                    else if (!string.IsNullOrEmpty(classNumber))
                                    {
                                        var classEntity = _unitOfWork.TblClass.GetFirstOrDefault(
                                            obj => obj.Class_Name == classNumber && obj.Class_Visible == "yes"
                                        );

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
                                            await _unitOfWork.Complete();
                                        }

                                        var newClassRoom = new TblClassRoom
                                        {
                                            Class_ID = classEntity.Class_ID,
                                            ClassRoom_Name = classRoomNumber,
                                            ClassRoom_Visible = "yes",
                                            ClassRoom_AddUserID = userId,
                                            ClassRoom_AddDate = DateTime.Now
                                        };
                                        _unitOfWork.TblClassRoom.Add(newClassRoom);
                                        await _unitOfWork.Complete();
                                        classRoomId = newClassRoom.ClassRoom_ID;
                                    }
                                }

                                if (!classRoomId.HasValue)
                                {
                                    errors.Add($"الصف {row}: لم يتم العثور على الفصل للطالب {studentName}");
                                    errorCount++;
                                    continue;
                                }

                                // Create student
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
                                await _unitOfWork.Complete();
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

                var message = $"تم استيراد {successCount} طالب بنجاح";
                if (errorCount > 0)
                {
                    message += $" | فشل استيراد {errorCount} طالب";
                }

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
                return Json(new { isValid = false, title = Title, message = "خطأ: " + ex.Message + " | Stack: " + ex.StackTrace });
            }
        }

        #endregion

        #region Archive (Deleted Students)

        [HttpGet]
        public IActionResult Archive()
        {
            try
            {
                // Get all classes for filter dropdown
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
                var studentsQuery = _unitOfWork.TblStudent.GetAll(
                    obj => obj.Student_Visible == "no", // الطلاب المحذوفين فقط
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

                var studentsList = studentsQuery.ToList();

                // Get user names for deleted by users
                var data = new List<object>();
                foreach (var s in studentsList)
                {
                    string deletedByName = "غير معروف";
                    if (!string.IsNullOrEmpty(s.Student_DeleteUserID))
                    {
                        var user = await _userManager.FindByIdAsync(s.Student_DeleteUserID);
                        deletedByName = user?.UserName ?? "غير معروف";
                    }

                    data.Add(new
                    {
                        student_ID = s.Student_ID,
                        student_Name = s.Student_Name,
                        student_Code = s.Student_Code,
                        student_Phone = s.Student_Phone,
                        student_Gender = s.Student_Gender,
                        className = s.ClassRoom?.Class?.Class_Name ?? "",
                        classRoomName = s.ClassRoom?.ClassRoom_Name ?? "",
                        deletedBy = deletedByName,
                        deletedDate = s.Student_DeleteDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
                        classRoom_ID = s.ClassRoom_ID
                    });
                }

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

                // استعادة الطالب - إرجاع Visible إلى yes
                student.Student_Visible = "yes";
                student.Student_EditUserID = _userManager.GetUserId(User);
                student.Student_EditDate = DateTime.Now;
                // نحتفظ بسجل الحذف للتاريخ
                // student.Student_DeleteUserID = null;
                // student.Student_DeleteDate = null;

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

                // حذف نهائي من قاعدة البيانات
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
        public async Task<IActionResult> PrintStudentCards(int? classId, int? classRoomId)
        {
            try
            {
                // 1. استعلام محسّن - نجلب فقط الحقول المطلوبة
                var studentsQuery = _unitOfWork.TblStudent.GetAll(
                    obj => obj.Student_Visible == "yes",
                    new[] { "ClassRoom.Class" } // include واحد فقط بدلاً من اثنين
                );

                // 2. تطبيق الفلتر على IQueryable قبل التنفيذ
                if (classId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom.Class_ID == classId.Value);
                }

                if (classRoomId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(s => s.ClassRoom_ID == classRoomId.Value);
                }

                // 3. Projection مباشر في الاستعلام (يجلب فقط الحقول المطلوبة)
                var students = await Task.Run(() => studentsQuery
                    //.AsNoTracking() // لا نحتاج tracking
                    .Select(s => new StudentCardViewModel
                    {
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        StudentPhone = s.Student_Phone,
                        ClassName = s.ClassRoom.Class.Class_Name ?? "",
                        ClassRoomName = s.ClassRoom.ClassRoom_Name ?? ""
                    })
                    .ToList());

                if (!students.Any())
                {
                    return Content("لا يوجد طلاب للطباعة");
                }

                // 4. توليد PDF في Thread منفصل
                byte[] pdfBytes = await Task.Run(() =>
                {
                    var pdfService = new ReportPdfService();
                    return pdfService.GenerateStudentCards(students);
                });

                return File(pdfBytes, "application/pdf", $"بطاقات_الطلاب_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return Content($"خطأ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintSingleStudentCard(int studentId)
        {
            try
            {
                // استعلام محسّن
                var student = await Task.Run(() => _unitOfWork.TblStudent
                    .GetAll(
                        obj => obj.Student_ID == studentId && obj.Student_Visible == "yes",
                        new[] { "ClassRoom.Class" }
                    )
                    //.AsNoTracking()
                    .Select(s => new StudentCardViewModel
                    {
                        StudentName = s.Student_Name,
                        StudentCode = s.Student_Code,
                        StudentPhone = s.Student_Phone,
                        ClassName = s.ClassRoom.Class.Class_Name ?? "",
                        ClassRoomName = s.ClassRoom.ClassRoom_Name ?? ""
                    })
                    .FirstOrDefault());

                if (student == null)
                {
                    return Content("الطالب غير موجود");
                }

                // توليد PDF في Thread منفصل
                byte[] pdfBytes = await Task.Run(() =>
                {
                    var pdfService = new ReportPdfService();
                    return pdfService.GenerateStudentCards(new List<StudentCardViewModel> { student });
                });

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
