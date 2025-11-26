using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SalesRepository.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TblBadgeDefinitions",
                columns: table => new
                {
                    Definition_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Badge_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Badge_Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Badge_Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Required_Points = table.Column<int>(type: "int", nullable: false),
                    Badge_Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Badge_Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Is_Active = table.Column<bool>(type: "bit", nullable: false),
                    Created_Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblBadgeDefinitions", x => x.Definition_ID);
                });

            migrationBuilder.CreateTable(
                name: "TblStudentBadges",
                columns: table => new
                {
                    Badge_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<int>(type: "int", nullable: false),
                    Badge_Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Badge_Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Earned_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Badge_Visible = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblStudentBadges", x => x.Badge_ID);
                    table.ForeignKey(
                        name: "FK_TblStudentBadges_TblStudent_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "TblStudent",
                        principalColumn: "Student_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TblStudentPoints",
                columns: table => new
                {
                    Point_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<int>(type: "int", nullable: false),
                    Total_Points = table.Column<int>(type: "int", nullable: false),
                    Monthly_Points = table.Column<int>(type: "int", nullable: false),
                    Attendance_Streak = table.Column<int>(type: "int", nullable: false),
                    Last_Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblStudentPoints", x => x.Point_ID);
                    table.ForeignKey(
                        name: "FK_TblStudentPoints_TblStudent_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "TblStudent",
                        principalColumn: "Student_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TblBadgeDefinitions",
                columns: new[] { "Definition_ID", "Badge_Color", "Badge_Icon", "Badge_Level", "Badge_Name", "Badge_Type", "Created_Date", "Description", "Is_Active", "Required_Points" },
                values: new object[,]
                {
                    { 1, "#CD7F32", "medal", "برونزي", "المنضبطة", "انضباط", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4561), "حضور منتظم لمدة أسبوعين", true, 50 },
                    { 2, "#C0C0C0", "medal", "فضي", "المنضبطة", "انضباط", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4674), "حضور منتظم لمدة شهر", true, 150 },
                    { 3, "#FFD700", "medal", "ذهبي", "المنضبطة", "انضباط", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4681), "حضور منتظم لمدة شهرين", true, 300 },
                    { 4, "#B9F2FF", "medal", "ماسي", "المنضبطة", "انضباط", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4687), "حضور منتظم لمدة فصل كامل", true, 500 },
                    { 5, "#CD7F32", "chart-simple", "برونزي", "المواظبة", "حضور_متتالي", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4692), "7 أيام حضور متتالي", true, 30 },
                    { 6, "#C0C0C0", "chart-simple", "فضي", "المواظبة", "حضور_متتالي", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4697), "14 يوم حضور متتالي", true, 70 },
                    { 7, "#FFD700", "chart-simple", "ذهبي", "المواظبة", "حضور_متتالي", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4702), "30 يوم حضور متتالي", true, 150 },
                    { 8, "#B9F2FF", "chart-simple", "ماسي", "المواظبة", "حضور_متتالي", new DateTime(2025, 11, 26, 9, 54, 1, 210, DateTimeKind.Local).AddTicks(4707), "60 يوم حضور متتالي", true, 300 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblStudentBadges_Badge_Type_Badge_Level",
                table: "TblStudentBadges",
                columns: new[] { "Badge_Type", "Badge_Level" });

            migrationBuilder.CreateIndex(
                name: "IX_TblStudentBadges_Student_ID",
                table: "TblStudentBadges",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TblStudentPoints_Student_ID",
                table: "TblStudentPoints",
                column: "Student_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TblStudentPoints_Total_Points",
                table: "TblStudentPoints",
                column: "Total_Points");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblBadgeDefinitions");

            migrationBuilder.DropTable(
                name: "TblStudentBadges");

            migrationBuilder.DropTable(
                name: "TblStudentPoints");
        }
    }
}
