using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;

namespace Sales.Controllers
{
	public class Product_CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IAuthorizationService _authorizationService;
		string Title = "الفئات";

        public Product_CategoryController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
			_userManager = userManager;
			_authorizationService = authorizationService;
		}

		[Authorize(Policy = "ProductCategory_View")]
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

		[HttpGet]
		public IActionResult GetCategory()
		{
			var category = _unitOfWork.Product_Category.GetAll(obj => obj.ProductCategory_Visible == "yes");
			return Json(new { data = category });
		}

		[HttpGet]
		public IActionResult Create()
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Create").Result).Succeeded)
				{
					return PartialView("_AuthorizedAdd");
				}
				return PartialView("_Product_CategoryCreate", new ModelProduct_Category());
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
		public async Task<IActionResult> Create(ModelProduct_Category modelProduct_Category)
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Create").Result).Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
				}
				var checkCategory = _unitOfWork.Product_Category.GetFirstOrDefault(obj => obj.ProductCategory_Name == modelProduct_Category.ProductCategory_Name.Trim() && obj.ProductCategory_Visible == "yes");
				if (checkCategory != null)
				{
					return Json(new { isValid = false, title = Title, message = "الفئة موجودة بالفعل" });
				}
				var category = new TblProduct_Category
				{
					ProductCategory_Name = modelProduct_Category.ProductCategory_Name.Trim(),
					ProductCategory_Visible = "yes",
					ProductCategory_AddUserID = _userManager.GetUserId(User),
					ProductCategory_AddDate = DateTime.Now
				};
				_unitOfWork.Product_Category.Add(category);

				if (await _unitOfWork.Complete() == 0)
				{
					return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
				}

				return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = category });
			}
			catch (Exception)
			{
				return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
			}
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Edit").Result).Succeeded)
				{
					return PartialView("_AuthorizedEdit");
				}
				var category = _unitOfWork.Product_Category.GetById(id);
				if (category == null)
				{
					ViewBag.Type = "error";
					ViewBag.Message = "الفئة غير موجودة";
					return View();
				}
				var model = new ModelProduct_Category
				{
					ProductCategory_ID = category.ProductCategory_ID,
					ProductCategory_Name = category.ProductCategory_Name
				};
				return PartialView("_Product_CategoryEdit", model);
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
		public async Task<IActionResult> Edit(ModelProduct_Category modelProduct_Category)
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Edit").Result).Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
				}
				var checkCategory = _unitOfWork.Product_Category.GetFirstOrDefault(obj => obj.ProductCategory_ID != modelProduct_Category.ProductCategory_ID && obj.ProductCategory_Name == modelProduct_Category.ProductCategory_Name.Trim() && obj.ProductCategory_Visible == "yes");
				if (checkCategory != null)
				{
					return Json(new { isValid = false, title = Title, message = "الفئة موجودة بالفعل" });
				}

				var category = _unitOfWork.Product_Category.GetById(modelProduct_Category.ProductCategory_ID);
				category.ProductCategory_Name = modelProduct_Category.ProductCategory_Name.Trim();
				category.ProductCategory_EditUserID = _userManager.GetUserId(User);
				category.ProductCategory_EditDate = DateTime.Now;

				if (await _unitOfWork.Complete() == 0)
				{
					return Json(new { isValid = false, title = Title, message = "لم يتم حفظ البيانات" });
				}

				return Json(new { isValid = true, title = Title, message = "تم الحفظ بنجاح", data = category });
			}
			catch (Exception)
			{
				return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
			}
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Delete").Result).Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
				}

				var category = _unitOfWork.Product_Category.GetById(id);
                category.ProductCategory_Visible = "no";
				category.ProductCategory_DeleteUserID = _userManager.GetUserId(User);
				category.ProductCategory_DeleteDate = DateTime.Now;
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

		[Authorize(Policy = "ProductCategory_Delete")]
		[HttpPost]
		public async Task<IActionResult> DeleteRange(List<string> lstId)
		{
			try
			{
				if (!(_authorizationService.AuthorizeAsync(User, "ProductCategory_Delete").Result).Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من وجود صلاحية لفتح هذة النافذة" });
				}
				string firstList = lstId[0].ToString();
				string[] lst = firstList.Split(",");
				await _unitOfWork.Product_Category.UpdateAll(obj => lst.Contains(obj.ProductCategory_ID.ToString()), obj => obj.SetProperty(obj => obj.ProductCategory_Visible, "no"));
				await _unitOfWork.Product_Category.UpdateAll(obj => lst.Contains(obj.ProductCategory_ID.ToString()), obj => obj.SetProperty(obj => obj.ProductCategory_DeleteUserID, _userManager.GetUserId(User)));
				await _unitOfWork.Product_Category.UpdateAll(obj => lst.Contains(obj.ProductCategory_ID.ToString()), obj => obj.SetProperty(obj => obj.ProductCategory_DeleteDate, DateTime.Now));
				return Json(new { isValid = true, title = Title, message = "تم الحذف بنجاح" });
			}
			catch (Exception)
			{
				return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
			}
		}

	}
}