using System.Security.Claims;

namespace SalesModel.ViewModels
{
	public class ModelPermissionItem
	{
		public static List<Claim> claimsList = new List<Claim>()
		{
			new Claim("المستخدمين","الإعدادات"),
			new Claim("UsersView","UsersView"),
			new Claim("UsersCreate","UsersCreate"),
			new Claim("UsersEdit","UsersEdit"),
			new Claim("UsersDelete","UsersDelete"),
            //-----------------------------------------------------------------------------------
			new Claim("صلاحيات المستخدمين","الإعدادات"),
			new Claim("UsersPermissionView","UsersPermissionView"),
			new Claim("UsersPermissionCreate","0"),
			new Claim("UsersPermissionEdit","UsersPermissionEdit"),
			new Claim("UsersPermissionDelete","0"),
            //-----------------------------------------------------------------------------------
            new Claim("الصفوف","Class"),
            new Claim("Class_View","Class_View"),
            new Claim("Class_Create","Class_Create"),
            new Claim("Class_Edit","Class_Edit"),
            new Claim("Class_Delete","Class_Delete"),

			//-----------------------------------------------------------------------------------
			new Claim("الفصول","ClassRoom"),
			new Claim("ClassRoom_View","ClassRoom_View"),
			new Claim("ClassRoom_Create","ClassRoom_Create"),
			new Claim("ClassRoom_Edit","ClassRoom_Edit"),
			new Claim("ClassRoom_Delete","ClassRoom_Delete"),

			//-----------------------------------------------------------------------------------
			new Claim("الطلاب","Student"),
			new Claim("Student_View","Student_View"),
			new Claim("Student_Create","Student_Create"),
			new Claim("Student_Edit","Student_Edit"),
			new Claim("Student_Delete","Student_Delete"),

            //-----------------------------------------------------------------------------------
			
			new Claim("اعدادات المدرسة","SchoolSettings"),
			new Claim("SchoolSettings.View","SchoolSettings.View"),
			new Claim("SchoolSettings.Edit","SchoolSettings.Edit"),
			new Claim("SchoolSettings.Edit","SchoolSettings.Edit"),
			new Claim("SchoolSettings.Edit","SchoolSettings.Edit"),

        };
	}
}
