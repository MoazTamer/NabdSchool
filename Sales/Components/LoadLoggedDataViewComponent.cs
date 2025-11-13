using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;

namespace Sales.Components
{
    public class LoadLoggedDataViewComponent : ViewComponent
    {
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IUnitOfWork _unitOfWork;

		public LoadLoggedDataViewComponent(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_unitOfWork = unitOfWork;
		}

		[HttpGet]
        public IViewComponentResult Invoke()
        {
			try
			{
				var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(obj => obj.Id == _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User));
				if (user == null || user.Visible == "no")
				{
					_signInManager.SignOutAsync();
				}

				var model = new ModelProfile()
				{
					UserName = user.UserName
				};
				return View(model);
			}
			catch (Exception)
			{
				return View();
			}
		}
    }
}
