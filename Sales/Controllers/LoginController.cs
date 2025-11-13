using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;

namespace Sales.Controllers
{
    public class LoginController : Controller
    {
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IUnitOfWork _unitOfWork;

		public LoginController(SignInManager<ApplicationUser> signInManager, IUnitOfWork unitOfWork)
		{
			_signInManager = signInManager;
			_unitOfWork = unitOfWork;
		}

		[HttpGet]
        public IActionResult Index(string? ReturnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
				{
					return LocalRedirect(ReturnUrl);
				}
				return LocalRedirect("~/");
			}
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ModelLogin modelLogin, string? ReturnUrl)
		{
			try
			{
				var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(obj => obj.UserName == modelLogin.UserName && obj.Password == modelLogin.Password && obj.Visible == "yes");
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(user.UserName, user.Password, isPersistent: true, lockoutOnFailure: false);
					if (result.Succeeded)
					{
						if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
						{
							return LocalRedirect(ReturnUrl);
						}
						return LocalRedirect("~/");
					}
				}
				
				ViewBag.Type = "error";
				ViewBag.Message = "من فضلك تأكد من إسم المستخدم وكلمة المرور";
				return View(modelLogin);
			}
			catch (Exception ex)
			{
				ViewBag.Type = "error";
				ViewBag.Message = "من فضلك تأكد من البيانات" + ex;
				return View(modelLogin);
			}
		}
	}
}
