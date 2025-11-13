using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;

namespace WillengDashboard.Area.Teacher.Controllers
{
	[Authorize]
	public class ProfileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		string Title = "تعديل بياناتى";

        public ProfileController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
			_signInManager = signInManager;
		}

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
				var user = _unitOfWork.ApplicationUser.GetById(_userManager.GetUserId(User));
				var model = new ModelProfile()
				{
					Category = user.Category,
					SignUserName = user.UserName
				};
				return View(model);
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
		public async Task<IActionResult> UserNameChange(ModelProfile modelProfile)
		{
			try
			{
				var user = _unitOfWork.ApplicationUser.GetById(_userManager.GetUserId(User));
				var check = await _userManager.CheckPasswordAsync(user, modelProfile.SignPassword);
				if (!check)
				{
					return Json(new { isValid = false, title = Title, message = "تأكد من كلمة المرور" });
				}
				var result = await _userManager.SetUserNameAsync(user, modelProfile.UserNameNew);
				if (!result.Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "عفوا ، لم يتم حفظ البيانات" });
				}
				await _signInManager.RefreshSignInAsync(user);
				return Json(new { isValid = true, title = Title, message = "تم حفظ التعديل بنجاح" });
			}
			catch (Exception)
			{
				return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PasswordChange(ModelProfile modelProfile)
		{
			try
			{
				var user = _unitOfWork.ApplicationUser.GetById(_userManager.GetUserId(User));
				var result = await _userManager.ChangePasswordAsync(user, modelProfile.PasswordCurrent, modelProfile.PasswordNew);
				if (!result.Succeeded)
				{
					return Json(new { isValid = false, title = Title, message = "تأكد من تسجيل كلمة مرور صحيحة" });
				}
				user.Password = modelProfile.PasswordNew;
				await _unitOfWork.Complete();
				await _signInManager.RefreshSignInAsync(user);
				return Json(new { isValid = true, title = Title, message = "تم حفظ التعديل بنجاح" });
			}
			catch (Exception)
			{
				return Json(new { isValid = false, title = Title, message = "من فضلك تأكد من البيانات" });
			}
		}
	}
}