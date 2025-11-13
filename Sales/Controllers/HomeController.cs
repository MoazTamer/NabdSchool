using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesModel.ViewModels;

namespace Sales.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
		private static TimeZoneInfo Arabian_Standard_Time =
			TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IAuthorizationService _authorizationService;
		private readonly IUnitOfWork _unitOfWork;

		public HomeController(SignInManager<ApplicationUser> signInManager, IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
		{
			_signInManager = signInManager;
			_unitOfWork = unitOfWork;
			_authorizationService = authorizationService;
		}

		[HttpGet]
		public IActionResult Index()
        {		
            return View();
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
	}
}
