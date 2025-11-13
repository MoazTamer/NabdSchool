namespace SalesModel.ViewModels
{
    public class ModelPermission
	{
		public ModelPermission()
		{
			Claims = new List<RolesClaim>();
		}
		public string UserID { get; set; }
		public string FullName { get; set; }
		public List<RolesClaim> Claims { get; set; }
	}

	public class RolesClaim
	{
		public string ClaimType { get; set; }
		public string ClaimValue { get; set; }
		public bool IsSelected { get; set; }
	}
}
