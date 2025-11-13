using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesRepository.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolSettingsClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TblSchoolSettings",
                columns: table => new
                {
                    Setting_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Setting_Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Setting_Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Setting_Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Setting_Visible = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Setting_AddUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Setting_AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Setting_EditUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Setting_EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Setting_DeleteUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Setting_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblSchoolSettings", x => x.Setting_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblSchoolSettings");
        }
    }
}
