using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesModel.IRepository;
using SalesModel.Models;
using System.Data;
using SalesModel.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Sales.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        string Title = "المستخدمين";

        public UsersController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = "Users_View")]
        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }


        // ============================================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult GetData()
        {
            try
            {
                // 1️⃣ استقبال المعاملات من DataTables
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                // 2️⃣ تحويل القيم
                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // 3️⃣ جلب كل المستخدمين المرئيين
                var allUsers = _unitOfWork.ApplicationUser
                    .GetAll(u => u.Visible == "yes")
                    .ToList();

                // 4️⃣ إجمالي السجلات قبل البحث
                int totalRecords = allUsers.Count;

                // 5️⃣ إضافة اسم الفرع لكل مستخدم
                var usersWithBranch = allUsers.Select(u => new
                {
                    User = u,
                    BranchName = u.Branch_ID > 0
                        ? _unitOfWork.Branch.GetById(u.Branch_ID)?.Branch_Name ?? "غير محدد"
                        : "غير محدد"
                }).ToList();

                // 6️⃣ البحث
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    usersWithBranch = usersWithBranch.Where(ub =>
                        (ub.User.UserName != null && ub.User.UserName.ToLower().Contains(searchValue)) ||
                        (ub.User.UserType != null && ub.User.UserType.ToLower().Contains(searchValue)) ||
                        (ub.BranchName != null && ub.BranchName.ToLower().Contains(searchValue))
                    ).ToList();
                }

                // 7️⃣ إجمالي السجلات بعد البحث
                int recordsFiltered = usersWithBranch.Count;

                // 8️⃣ الترتيب
                if (!string.IsNullOrEmpty(sortColumnIndex))
                {
                    int columnIndex = Convert.ToInt32(sortColumnIndex);
                    bool isAscending = sortDirection == "asc";

                    switch (columnIndex)
                    {
                        case 1: // Branch Name
                            usersWithBranch = isAscending
                                ? usersWithBranch.OrderBy(ub => ub.BranchName).ToList()
                                : usersWithBranch.OrderByDescending(ub => ub.BranchName).ToList();
                            break;
                        case 2: // User Name
                            usersWithBranch = isAscending
                                ? usersWithBranch.OrderBy(ub => ub.User.UserName).ToList()
                                : usersWithBranch.OrderByDescending(ub => ub.User.UserName).ToList();
                            break;
                        case 3: // User Type
                            usersWithBranch = isAscending
                                ? usersWithBranch.OrderBy(ub => ub.User.UserType).ToList()
                                : usersWithBranch.OrderByDescending(ub => ub.User.UserType).ToList();
                            break;
                        default:
                            usersWithBranch = usersWithBranch.OrderBy(ub => ub.User.UserName).ToList();
                            break;
                    }
                }
                else
                {
                    usersWithBranch = usersWithBranch.OrderBy(ub => ub.User.UserName).ToList();
                }

                // 9️⃣ التصفح (Pagination)
                var pagedData = usersWithBranch.Skip(skip).Take(pageSize).ToList();

                // 🔟 تحويل للـ Response Format
                var data = pagedData.Select(ub => new
                {
                    id = ub.User.Id,
                    branchName = ub.BranchName,
                    userName = ub.User.UserName,
                    userType = ub.User.UserType,
                    visible = ub.User.Visible
                }).ToList();

                // 1️⃣1️⃣ إرجاع النتيجة
                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = recordsFiltered,
                    data = data
                });
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                System.Diagnostics.Debug.WriteLine($"GetData Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // إرجاع استجابة فارغة في حالة الخطأ
                return Json(new
                {
                    draw = Request.Form["draw"].FirstOrDefault(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GetUser()
        {
            try
            {
                var users = _unitOfWork.SP_Call.List<ModelUsers>("SPUsers")
                    .Where(u => u.Visible == "yes")
                    .Select(u => new
                    {
                        id = u.Id,
                        branchName = u.BranchName,
                        userName = u.UserName,
                        UserType = u.UserType,
                        visible = u.Visible
                    })
                    .ToList();

                return Json(new { data = users });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUser: {ex.Message}");
                return Json(new { data = new List<object>() });
            }
        }


        [HttpPost]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _unitOfWork.ApplicationUser.GetAll(
                    u => u.Visible == "yes",
                    includeProperties: new string[] { "TblBranch" }
                )
                .Select(u => new
                {
                    id = u.Id,
                    branchName = u.TblBranch != null ? u.TblBranch.Branch_Name : "غير محدد",
                    userName = u.UserName,
                    userType = u.UserType,
                    visible = u.Visible
                })
                .ToList();

                return Json(new { data = users });
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<object>() });
            }
        }


        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Create").Result).Succeeded)
                {
                    return PartialView("_AuthorizedAdd");
                }

                var model = new ModelUsers
                {
                    BranchList = _unitOfWork.Branch.GetAll(obj => obj.Branch_Visible == "yes").Select(i => new SelectListItem
                    {
                        Text = i.Branch_Name,
                        Value = i.Branch_ID.ToString()
                    }),
                    UserTypeList = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "مدرس", Value = "Teacher" },
                        new SelectListItem { Text = "مدخل بيانات", Value = "DataEntry" },
                        new SelectListItem { Text = "إدارة", Value = "Admin" }
                    }
                };
                return PartialView("_Users_Create", model);
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
        public async Task<IActionResult> Create(ModelUsers modelUsers)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Create").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var branch = _unitOfWork.Branch.GetById(modelUsers.BranchID);
                if (branch == null)
                {
                    return Json(new { isValid = false, title = Title, message = "الفرع غير موجود" });
                }

                var existingUser = await _userManager.FindByNameAsync(modelUsers.UserName);
                if (existingUser != null)
                {
                    return Json(new { isValid = false, title = Title, message = "اسم المستخدم موجود بالفعل" });
                }

                if (string.IsNullOrEmpty(modelUsers.UserType))
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك اختر نوع المستخدم" });
                }

                var user = new ApplicationUser
                {
                    Branch_ID = modelUsers.BranchID,
                    UserName = modelUsers.UserName,
                    Email = $"{modelUsers.UserName}@domain.com",
                    UserType = modelUsers.UserType,
                    Password = "123456",
                    Category = modelUsers.BranchID == 1 ? "admin" : "assistant",
                    Visible = "yes"
                };

                var result = await _userManager.CreateAsync(user, user.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { isValid = false, title = Title, message = $"فشل إنشاء المستخدم: {errors}" });
                }

                // ✅ فقط أضف المستخدم للـ Role (بدون إنشاء Role جديد)
                var roleExists = await _roleManager.RoleExistsAsync(user.UserType);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, user.UserType);
                }
                //else
                //{
                //    return Json(new { isValid = false, title = Title, message = $"الصلاحية {user.UserType} غير موجودة في النظام" });
                //}

                var userResponse = new
                {
                    id = user.Id,
                    branchName = branch.Branch_Name,
                    userName = user.UserName,
                    userType = user.UserType,
                    visible = user.Visible
                };

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = userResponse });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { isValid = false, title = Title, message = $"حدث خطأ: {innerMessage}" });
            }
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Edit").Result).Succeeded)
                {
                    return PartialView("_AuthorizedEdit");
                }

                var user = _unitOfWork.ApplicationUser.GetById(id);
                if (user == null)
                {
                    ViewBag.Type = "error";
                    ViewBag.Message = "المستخدم غير موجود";
                    return View();
                }

                var model = new ModelUsers
                {
                    Id = user.Id,
                    BranchID = user.Branch_ID,
                    UserName = user.UserName,
                    UserType = user.UserType,
                    Password = user.Password,
                    BranchList = _unitOfWork.Branch.GetAll(obj => obj.Branch_Visible == "yes").Select(i => new SelectListItem
                    {
                        Text = i.Branch_Name,
                        Value = i.Branch_ID.ToString()
                    }),
                    UserTypeList = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "مدرس", Value = "Teacher" },
                        new SelectListItem { Text = "مدخل بيانات", Value = "DataEntry" },
                        new SelectListItem { Text = "إدارة", Value = "Admin" }

                    }
                };
                return PartialView("_Users_Edit", model);
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        // ============================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ModelUsers modelUsers)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Edit").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var user = _unitOfWork.ApplicationUser.GetById(modelUsers.Id);

                if (user == null)
                {
                    return Json(new { isValid = false, title = Title, message = "المستخدم غير موجود" });
                }

                // تعديل اسم المستخدم
                if (user.UserName != modelUsers.UserName)
                {
                    // التحقق من أن الاسم الجديد غير مستخدم
                    var existingUser = await _userManager.FindByNameAsync(modelUsers.UserName);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        return Json(new { isValid = false, title = Title, message = "اسم المستخدم موجود بالفعل" });
                    }

                    var result = await _userManager.SetUserNameAsync(user, modelUsers.UserName);
                    if (!result.Succeeded)
                    {
                        return Json(new { isValid = false, title = Title, message = "عفوا، لم يتم تعديل إسم المستخدم" });
                    }
                }

                // تعديل كلمة المرور (فقط إذا تم إدخال كلمة مرور جديدة)
                if (!string.IsNullOrWhiteSpace(modelUsers.Password) && user.Password != modelUsers.Password)
                {
                    var result = await _userManager.ChangePasswordAsync(user, user.Password, modelUsers.Password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        return Json(new { isValid = false, title = Title, message = $"عفوا، لم يتم تعديل كلمة المرور: {errors}" });
                    }
                    user.Password = modelUsers.Password;
                }

                // تعديل الفرع
                if (user.Branch_ID != modelUsers.BranchID)
                {
                    var branch = _unitOfWork.Branch.GetById(modelUsers.BranchID);
                    if (branch == null)
                    {
                        return Json(new { isValid = false, title = Title, message = "الفرع غير موجود" });
                    }

                    user.Branch_ID = modelUsers.BranchID;
                    user.Category = modelUsers.BranchID == 1 ? "admin" : "assistant";
                }

                // تحديث نوع المستخدم والـ Role
                if (user.UserType != modelUsers.UserType)
                {
                    // التحقق من وجود الـ Role الجديد
                    var roleExists = await _roleManager.RoleExistsAsync(modelUsers.UserType);
                    if (!roleExists)
                    {
                        var newRole = new ApplicationRole
                        {
                            Name = modelUsers.UserType,
                            NormalizedName = modelUsers.UserType.ToUpper()
                        };
                        await _roleManager.CreateAsync(newRole);
                    }

                    // إزالة الـ Role القديم
                    var oldRoles = await _userManager.GetRolesAsync(user);
                    if (oldRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, oldRoles);
                    }

                    // إضافة الـ Role الجديد
                    user.UserType = modelUsers.UserType;
                    await _userManager.AddToRoleAsync(user, user.UserType);
                }

                await _unitOfWork.Complete();

                // جلب البيانات المحدثة للعرض
                var userLoad = _unitOfWork.ApplicationUser.GetFirstOrDefault(
                    u => u.Id == user.Id,
                    includeProperties: new string[] { "TblBranch" }
                );

                var userResponse = new
                {
                    id = userLoad.Id,
                    branchName = userLoad.TblBranch?.Branch_Name ?? "غير محدد",
                    userName = userLoad.UserName,
                    userType = userLoad.UserType,
                    visible = userLoad.Visible
                };

                return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = userResponse });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(ModelUsers modelUsers)
        //{
        //    try
        //    {
        //        if (!(_authorizationService.AuthorizeAsync(User, "Users_Edit").Result).Succeeded)
        //        {
        //            return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
        //        }

        //        var user = _unitOfWork.ApplicationUser.GetById(modelUsers.Id);

        //        if (user.UserName != modelUsers.UserName)
        //        {
        //            var result = await _userManager.SetUserNameAsync(user, modelUsers.UserName);
        //            if (!result.Succeeded)
        //            {
        //                return Json(new { isValid = false, title = Title, message = "عفوا , لم يتم تعديل إسم المستخدم" });
        //            }
        //        }

        //        if (user.Password != modelUsers.Password)
        //        {
        //            var result = await _userManager.ChangePasswordAsync(user, user.Password, modelUsers.Password);
        //            if (!result.Succeeded)
        //            {
        //                return Json(new { isValid = false, title = Title, message = "عفوا , لم يتم تعديل كلمة المرور" });
        //            }
        //            user.Password = modelUsers.Password;
        //        }

        //        if (user.Branch_ID != modelUsers.BranchID)
        //        {
        //            user.Branch_ID = modelUsers.BranchID;
        //            user.Category = modelUsers.BranchID == 1 ? "admin" : "assistant";
        //        }

        //        // ✅ تحديث نوع المستخدم والـ Role
        //        if (user.UserType != modelUsers.UserType)
        //        {
        //            // إزالة الـ Role القديم
        //            var oldRoles = await _userManager.GetRolesAsync(user);
        //            if (oldRoles.Any())
        //            {
        //                await _userManager.RemoveFromRolesAsync(user, oldRoles);
        //            }

        //            // إضافة الـ Role الجديد
        //            user.UserType = modelUsers.UserType;
        //            await _userManager.AddToRoleAsync(user, user.UserType);
        //        }

        //        await _unitOfWork.Complete();

        //        var userLoad = _unitOfWork.ApplicationUser.GetFirstOrDefault(
        //            u => u.Id == user.Id,
        //            includeProperties: new string[] { "TblBranch" }
        //        );

        //        var userResponse = new
        //        {
        //            id = userLoad.Id,
        //            branchName = userLoad.TblBranch?.Branch_Name,
        //            userName = userLoad.UserName,
        //            userType = userLoad.UserType,
        //            visible = userLoad.Visible
        //        };

        //        return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = userResponse });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات: " + ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Delete").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                var user = _unitOfWork.ApplicationUser.GetById(id);
                user.NormalizedUserName = id;
                user.Visible = "no";

                if (await _unitOfWork.Complete() == 0)
                {
                    return Json(new { isValid = false, title = Title, message = "عفوا ، لم يتم الحذف" });
                }

                return Json(new { isValid = true, title = Title, message = "تم الحذف بنجاح" });
            }
            catch (Exception)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
            }
        }

        [Authorize(Policy = "Users_Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteRange(List<string> lstId)
        {
            try
            {
                if (!(_authorizationService.AuthorizeAsync(User, "Users_Delete").Result).Succeeded)
                {
                    return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
                }

                string firstList = lstId[0].ToString();
                string[] lst = firstList.Split(",");
                await _unitOfWork.ApplicationUser.UpdateAll(obj => lst.Contains(obj.Id), obj => obj.SetProperty(obj => obj.Visible, "no"));
                return Json(new { isValid = true, title = Title, message = "تم الحذف بنجاح" });
            }
            catch (Exception)
            {
                return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
            }
        }

        // ... باقي الكود Permission بدون تغيير ...

        [Authorize(Policy = "UsersPermission_View")]
        [HttpGet]
        public IActionResult Permission(string id)
        {
            try
            {
                var user = _unitOfWork.ApplicationUser.GetById(id);
                if (user == null || user.Visible == "no")
                {
                    ViewBag.Type = "error";
                    ViewBag.Message = "هذا المستخدم غير موجود";
                    return View();
                }

                var existingUserClaims = _userManager.GetClaimsAsync(user).Result;
                var model = new ModelPermission()
                {
                    UserID = user.Id,
                    FullName = user.UserName
                };

                foreach (Claim claim in ModelPermissionItem.claimsList)
                {
                    RolesClaim userClaim = new RolesClaim
                    {
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    };
                    if (existingUserClaims.Any(c => c.Type == claim.Type))
                    {
                        userClaim.IsSelected = true;
                    }
                    model.Claims.Add(userClaim);
                }
                return View(model);
            }
            catch (Exception)
            {
                ViewBag.Type = "error";
                ViewBag.Message = "من فضلك تأكد من البيانات";
                return View();
            }
        }

        [Authorize(Policy = "UsersPermission_Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permission(ModelPermission modelPermission)
        {
            var user = _unitOfWork.ApplicationUser.GetById(modelPermission.UserID);
            if (user == null || user.Visible == "no")
            {
                return Json(new { isValid = false, title = Title, message = "هذا المستخدم غير موجود" });
            }

            var claims = _userManager.GetClaimsAsync(user).Result;
            for (int i = 0; i < modelPermission.Claims.Count(); i++)
            {
                var claim = (from c in claims
                             where c.Type == modelPermission.Claims[i].ClaimType
                             select c).FirstOrDefault();
                if (claim != null)
                {
                    _userManager.RemoveClaimAsync(user, claim).Wait();
                }
            }

            await _userManager.AddClaimsAsync(user, modelPermission.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.IsSelected.ToString())));
            await _userManager.UpdateSecurityStampAsync(user);

            return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح" });
        }
    }
}