using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NabdSchool.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الأول المتوسط", 7, "متوسط" });

            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الثاني المتوسط", 8, "متوسط" });

            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الثالث المتوسط", 9, "متوسط" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الأول", 1, "ابتدائي" });

            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الثاني", 2, "ابتدائي" });

            migrationBuilder.UpdateData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GradeName", "GradeNumber", "Stage" },
                values: new object[] { "الصف الثالث", 3, "ابتدائي" });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "GradeName", "GradeNumber", "IsActive", "Stage" },
                values: new object[,]
                {
                    { 4, "الصف الرابع", 4, true, "ابتدائي" },
                    { 5, "الصف الخامس", 5, true, "ابتدائي" },
                    { 6, "الصف السادس", 6, true, "ابتدائي" },
                    { 7, "الصف السابع", 7, true, "متوسط" },
                    { 8, "الصف الثامن", 8, true, "متوسط" },
                    { 9, "الصف التاسع", 9, true, "متوسط" },
                    { 10, "الصف العاشر", 10, true, "ثانوي" },
                    { 11, "الصف الحادي عشر", 11, true, "ثانوي" },
                    { 12, "الصف الثاني عشر", 12, true, "ثانوي" }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "Capacity", "ClassName", "ClassNumber", "ClassTeacher", "GradeId", "IsActive" },
                values: new object[,]
                {
                    { 16, 30, "الفصل 1", 1, null, 4, true },
                    { 17, 30, "الفصل 2", 2, null, 4, true },
                    { 18, 30, "الفصل 3", 3, null, 4, true },
                    { 19, 30, "الفصل 4", 4, null, 4, true },
                    { 20, 30, "الفصل 5", 5, null, 4, true },
                    { 21, 30, "الفصل 1", 1, null, 5, true },
                    { 22, 30, "الفصل 2", 2, null, 5, true },
                    { 23, 30, "الفصل 3", 3, null, 5, true },
                    { 24, 30, "الفصل 4", 4, null, 5, true },
                    { 25, 30, "الفصل 5", 5, null, 5, true },
                    { 26, 30, "الفصل 1", 1, null, 6, true },
                    { 27, 30, "الفصل 2", 2, null, 6, true },
                    { 28, 30, "الفصل 3", 3, null, 6, true },
                    { 29, 30, "الفصل 4", 4, null, 6, true },
                    { 30, 30, "الفصل 5", 5, null, 6, true },
                    { 31, 30, "الفصل 1", 1, null, 7, true },
                    { 32, 30, "الفصل 2", 2, null, 7, true },
                    { 33, 30, "الفصل 3", 3, null, 7, true },
                    { 34, 30, "الفصل 4", 4, null, 7, true },
                    { 35, 30, "الفصل 5", 5, null, 7, true },
                    { 36, 30, "الفصل 1", 1, null, 8, true },
                    { 37, 30, "الفصل 2", 2, null, 8, true },
                    { 38, 30, "الفصل 3", 3, null, 8, true },
                    { 39, 30, "الفصل 4", 4, null, 8, true },
                    { 40, 30, "الفصل 5", 5, null, 8, true },
                    { 41, 30, "الفصل 1", 1, null, 9, true },
                    { 42, 30, "الفصل 2", 2, null, 9, true },
                    { 43, 30, "الفصل 3", 3, null, 9, true },
                    { 44, 30, "الفصل 4", 4, null, 9, true },
                    { 45, 30, "الفصل 5", 5, null, 9, true },
                    { 46, 30, "الفصل 1", 1, null, 10, true },
                    { 47, 30, "الفصل 2", 2, null, 10, true },
                    { 48, 30, "الفصل 3", 3, null, 10, true },
                    { 49, 30, "الفصل 4", 4, null, 10, true },
                    { 50, 30, "الفصل 5", 5, null, 10, true },
                    { 51, 30, "الفصل 1", 1, null, 11, true },
                    { 52, 30, "الفصل 2", 2, null, 11, true },
                    { 53, 30, "الفصل 3", 3, null, 11, true },
                    { 54, 30, "الفصل 4", 4, null, 11, true },
                    { 55, 30, "الفصل 5", 5, null, 11, true },
                    { 56, 30, "الفصل 1", 1, null, 12, true },
                    { 57, 30, "الفصل 2", 2, null, 12, true },
                    { 58, 30, "الفصل 3", 3, null, 12, true },
                    { 59, 30, "الفصل 4", 4, null, 12, true },
                    { 60, 30, "الفصل 5", 5, null, 12, true }
                });
        }
    }
}
