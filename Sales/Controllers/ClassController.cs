using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;
using System.Formats.Asn1;
using OfficeOpenXml;
using System.Globalization;

namespace Sales.Controllers
{
    public class ClassController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        string Title = "الصفوف";

        public ClassController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = "Class_View")]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
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
        public IActionResult GetClasses()
        {
            try
            {
                var classes = _unitOfWork.TblClass.GetAll(obj => obj.Class_Visible == "yes").ToList();

                var data = classes.Select(c => new
                {
                    class_ID = c.Class_ID,
                    class_Name = c.Class_Name,
                    classRooms = _unitOfWork.TblClassRoom.GetAll(cr => cr.Class_ID == c.Class_ID && cr.ClassRoom_Visible == "yes")
                        .Select(cr => new
                        {
                            classRoom_ID = cr.ClassRoom_ID,
                            classRoom_Name = cr.ClassRoom_Name,
                            studentsCount = _unitOfWork.TblStudent != null
                                ? _unitOfWork.TblStudent.GetAll(s => s.ClassRoom_ID == cr.ClassRoom_ID && s.Student_Visible == "yes").Count()
                                : 0
                        }).ToList()
                }).ToList();

                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

        #region Class CRUD

        [Authorize (Policy = "Class_Create")]
        [HttpGet]
        public IActionResult CreateClass()
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Class_Create").Result).Succeeded)
                {
                    return PartialView("_AuthorizedAdd");
                }
                return PartialView("_ClassCreate", new ModelClass());
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(ModelClass model)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Class_Create").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var checkClass = _unitOfWork.TblClass.GetFirstOrDefault(obj => obj.Class_Name == model.Class_Name.Trim() && obj.Class_Visible == "yes");
                if (checkClass != null)
                {
                    return Json(new { isValid = false, title = Title, message = "الصف موجود بالفعل" });
                }

                var classEntity = new TblClass
                {
                    Class_Name = model.Class_Name.Trim(),
                    Class_Visible = "yes",
                    Class_AddUserID = _userManager.GetUserId(User),
                    Class_AddDate = DateTime.Now
                };

                _unitOfWork.TblClass.Add(classEntity);

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = classEntity });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditClass(int id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Class_Edit").Result).Succeeded)
                {
                    return PartialView("_AuthorizedEdit");
                }

                var classEntity = _unitOfWork.TblClass.GetById(id);
                if (classEntity == null)
                {
                    ViewBag.Type = "error";
                    ViewBag.Message = "الصف غير موجود";
                    return View();
                }

                var model = new ModelClass
                {
                    Class_ID = classEntity.Class_ID,
                    Class_Name = classEntity.Class_Name
                };

                return PartialView("_ClassEdit", model);
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(ModelClass model)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Class_Edit").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var checkClass = _unitOfWork.TblClass.GetFirstOrDefault(obj => obj.Class_ID != model.Class_ID && obj.Class_Name == model.Class_Name.Trim() && obj.Class_Visible == "yes");
                if (checkClass != null)
                {
                    return Json(new { isValid = false, title = Title, message = "الصف موجود بالفعل" });
                }

                var classEntity = _unitOfWork.TblClass.GetById(model.Class_ID);
                classEntity.Class_Name = model.Class_Name.Trim();
                classEntity.Class_EditUserID = _userManager.GetUserId(User);
                classEntity.Class_EditDate = DateTime.Now;

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = classEntity });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Class_Delete").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var classEntity = _unitOfWork.TblClass.GetById(id);
                classEntity.Class_Visible = "no";
                classEntity.Class_DeleteUserID = _userManager.GetUserId(User);
                classEntity.Class_DeleteDate = DateTime.Now;

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

        #region ClassRoom CRUD

        [HttpGet]
        public IActionResult CreateClassRoom(int classId)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "ClassRoom_Create").Result).Succeeded)
                {
                    return PartialView("_AuthorizedAdd");
                }

                var model = new ModelClassRoom { Class_ID = classId };
                return PartialView("_ClassRoomCreate", model);
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClassRoom(ModelClassRoom model)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "ClassRoom_Create").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var checkClassRoom = _unitOfWork.TblClassRoom.GetFirstOrDefault(obj => obj.Class_ID == model.Class_ID && obj.ClassRoom_Name == model.ClassRoom_Name.Trim() && obj.ClassRoom_Visible == "yes");
                if (checkClassRoom != null)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "الفصل موجود بالفعل" });
                }

                var classRoom = new TblClassRoom
                {
                    Class_ID = model.Class_ID,
                    ClassRoom_Name = model.ClassRoom_Name.Trim(),
                    ClassRoom_Visible = "yes",
                    ClassRoom_AddUserID = _userManager.GetUserId(User),
                    ClassRoom_AddDate = DateTime.Now
                };

                _unitOfWork.TblClassRoom.Add(classRoom);

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = "الفصول", message = "تم الحفظ بنجاح", data = classRoom });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditClassRoom(int id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "ClassRoom_Edit").Result).Succeeded)
                {
                    return PartialView("_AuthorizedEdit");
                }

                var classRoom = _unitOfWork.TblClassRoom.GetById(id);

                if (classRoom == null)
                {
                    ViewBag.Type = "error";
                    ViewBag.Message = "الفصل غير موجود";
                    return View();
                }

                var classEntity = _unitOfWork.TblClass.GetById(classRoom.Class_ID);

                var model = new ModelClassRoom
                {
                    ClassRoom_ID = classRoom.ClassRoom_ID,
                    Class_ID = classRoom.Class_ID,
                    ClassRoom_Name = classRoom.ClassRoom_Name,
                    Class_Name = classEntity?.Class_Name
                };

                return PartialView("_ClassRoomEdit", model);
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassRoom(ModelClassRoom model)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "ClassRoom_Edit").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var checkClassRoom = _unitOfWork.TblClassRoom.GetFirstOrDefault(obj => obj.ClassRoom_ID != model.ClassRoom_ID && obj.Class_ID == model.Class_ID && obj.ClassRoom_Name == model.ClassRoom_Name.Trim() && obj.ClassRoom_Visible == "yes");
                if (checkClassRoom != null)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "الفصل موجود بالفعل" });
                }

                var classRoom = _unitOfWork.TblClassRoom.GetById(model.ClassRoom_ID);
                classRoom.ClassRoom_Name = model.ClassRoom_Name.Trim();
                classRoom.ClassRoom_EditUserID = _userManager.GetUserId(User);
                classRoom.ClassRoom_EditDate = DateTime.Now;

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "لم يتم حفظ البيانات" });
                }

                return Json(new { isValid = true, title = "الفصول", message = "تم الحفظ بنجاح", data = classRoom });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClassRoom(int id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "ClassRoom_Delete").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var classRoom = _unitOfWork.TblClassRoom.GetById(id);
                classRoom.ClassRoom_Visible = "no";
                classRoom.ClassRoom_DeleteUserID = _userManager.GetUserId(User);
                classRoom.ClassRoom_DeleteDate = DateTime.Now;

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = "الفصول", message = "عفوا ، لم يتم الحذف" });
                }

                return Json(new { isValid = true, title = "الفصول", message = "تم الحذف بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = "الفصول", message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }

        #endregion


    }
}
