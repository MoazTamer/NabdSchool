using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NabdSchool.BL.Services;
using NabdSchool.Core.Entities;
using NabdSchool.Web.ViewModels;

namespace NabdSchool.Web.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: Students
        [HttpGet]
        public async Task<IActionResult> Index(int? grade, int? classNumber, bool showDeleted = false)
        {
            IEnumerable<Student> students;

            if (grade.HasValue && classNumber.HasValue)
            {
                students = await _studentService.GetStudentsByClassAsync(grade.Value, classNumber.Value, showDeleted);
            }
            else if (grade.HasValue)
            {
                students = await _studentService.GetStudentsByGradeAsync(grade.Value, showDeleted);
            }
            else
            {
                students = await _studentService.GetAllStudentsAsync(showDeleted);
            }

            ViewBag.ShowDeleted = showDeleted;
            ViewBag.SelectedGrade = grade;
            ViewBag.SelectedClass = classNumber;

            return View(students);
        }

        // GET: Students/Create
        [HttpGet]
        [Authorize(Roles = "Admin,DataEntry")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,DataEntry")]
        public async Task<IActionResult> Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _studentService.StudentNumberExistsAsync(model.StudentNumber))
                {
                    ModelState.AddModelError("StudentNumber", "رقم الطالب موجود مسبقاً");
                    return View(model);
                }

                var student = new Student
                {
                    StudentNumber = model.StudentNumber,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    GradeId = model.GradeId,
                    ClassId = model.ClassId
                };

                await _studentService.CreateStudentAsync(student, User.Identity.Name);
                TempData["Success"] = "تم إضافة الطالب بنجاح";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Students/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,DataEntry")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound();

            var model = new StudentViewModel
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                PhoneNumber = student.PhoneNumber,
                GradeId = student.GradeId,
                ClassId = student.ClassId
            };

            return View(model);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,DataEntry")]
        public async Task<IActionResult> Edit(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _studentService.StudentNumberExistsAsync(model.StudentNumber, model.Id))
                {
                    ModelState.AddModelError("StudentNumber", "رقم الطالب موجود مسبقاً");
                    return View(model);
                }

                var student = new Student
                {
                    Id = model.Id,
                    StudentNumber = model.StudentNumber,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    GradeId = model.GradeId,
                    ClassId = model.ClassId
                };

                var result = await _studentService.UpdateStudentAsync(student, User.Identity.Name);
                if (result)
                {
                    TempData["Success"] = "تم تحديث بيانات الطالب بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "حدث خطأ أثناء التحديث";
                }
            }

            return View(model);
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _studentService.SoftDeleteStudentAsync(id, User.Identity.Name);
            if (result)
            {
                TempData["Success"] = "تم حذف الطالب بنجاح (نقل للأرشيف)";
            }
            else
            {
                TempData["Error"] = "حدث خطأ أثناء الحذف";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Students/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _studentService.RestoreStudentAsync(id, User.Identity.Name);
            if (result)
            {
                TempData["Success"] = "تم استرجاع الطالب من الأرشيف بنجاح";
            }
            else
            {
                TempData["Error"] = "حدث خطأ أثناء الاسترجاع";
            }

            return RedirectToAction(nameof(Index), new { showDeleted = true });
        }

        // GET: Students/Import
        [HttpGet]
        [Authorize(Roles = "Admin,DataEntry")]
        public IActionResult Import()
        {
            return View();
        }

        // POST: Students/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,DataEntry")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "الرجاء اختيار ملف Excel";
                return View();
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["Error"] = "يجب أن يكون الملف من نوع Excel (.xlsx أو .xls)";
                return View();
            }

            using var stream = file.OpenReadStream();
            var result = await _studentService.ImportFromExcelAsync(stream, User.Identity.Name);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }

        // GET: Students/Archive
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive()
        {
            var deletedStudents = await _studentService.GetAllStudentsAsync(includeDeleted: true);
            var archivedStudents = deletedStudents.Where(s => !s.IsVisible);

            return View(archivedStudents);
        }
    }
}
