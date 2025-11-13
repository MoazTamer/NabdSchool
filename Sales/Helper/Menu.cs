using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sales.Helper
{
	public static class Menu
	{
		public static string? IsSelected(this IHtmlHelper htmlHelper, string controllers, string actions, string cssClass = "menu-link active")
		{
			string currentAction = htmlHelper.ViewContext.RouteData.Values["action"].ToString().ToLower();
			string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();

			IEnumerable<string> acceptedActions = (actions ?? currentAction).Split(',');
			IEnumerable<string> acceptedControllers = (controllers ?? currentController).Split(',');

			return acceptedActions.Contains(currentAction) && acceptedControllers.Contains(currentController) ?
				cssClass : "menu-link";
		}

		public static string? IsShow(this IHtmlHelper htmlHelper, string controllers)
		{
			string currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString().ToLower();

			switch (currentController)
			{
				case "users":
					return "setting";
				case "product_category":
					return "product";
				default:
                    return "";
            }
		}
	}

	
}
