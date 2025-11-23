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
            new Claim("Class_View1","Class_View1"),
            new Claim("Class_Create1","Class_Create1"),
            new Claim("Class_Edit1","Class_Edit1"),
            new Claim("Class_Delete1","Class_Delete1"),

			//-----------------------------------------------------------------------------------
			new Claim("الفصول","ClassRoom"),
			new Claim("ClassRoom_View1","ClassRoom_View1"),
			new Claim("ClassRoom_Create1","ClassRoom_Create1"),
			new Claim("ClassRoom_Edit1","ClassRoom_Edit1"),
			new Claim("ClassRoom_Delete1","ClassRoom_Delete1"),

			//-----------------------------------------------------------------------------------
			new Claim("الطلاب","Student"),
			new Claim("Student_View1","Student_View1"),
			new Claim("Student_Create1","Student_Create1"),
			new Claim("Student_Edit1","Student_Edit1"),
			new Claim("Student_Delete1","Student_Delete1"),

            //-----------------------------------------------------------------------------------
			
			new Claim("اعدادات المدرسة","اعدادات المدرسة"),
			new Claim("SchoolSettings.View1","SchoolSettings.View"),
			new Claim("SchoolSettings.Edit1","0"),
			new Claim("SchoolSettings.Edit1","SchoolSettings.Edit1"),
			new Claim("SchoolSettings.Edit1","0"),

        };
	}
}
