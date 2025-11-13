using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesRepository.Migrations
{
    /// <inheritdoc />
    public partial class AddClassAndClassRoomTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Grade");

            migrationBuilder.CreateTable(
                name: "TblClass",
                columns: table => new
                {
                    Class_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Class_Visible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_AddUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Class_EditUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Class_DeleteUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblClass", x => x.Class_ID);
                });

            migrationBuilder.CreateTable(
                name: "TblClassRoom",
                columns: table => new
                {
                    ClassRoom_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class_ID = table.Column<int>(type: "int", nullable: false),
                    ClassRoom_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassRoom_Visible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoom_AddUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoom_AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassRoom_EditUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoom_EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassRoom_DeleteUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassRoom_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblClassRoom", x => x.ClassRoom_ID);
                    table.ForeignKey(
                        name: "FK_TblClassRoom_TblClass_Class_ID",
                        column: x => x.Class_ID,
                        principalTable: "TblClass",
                        principalColumn: "Class_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblStudent",
                columns: table => new
                {
                    Student_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassRoom_ID = table.Column<int>(type: "int", nullable: false),
                    Student_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Student_Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_Visible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_AddUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Student_EditUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Student_DeleteUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Student_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblStudent", x => x.Student_ID);
                    table.ForeignKey(
                        name: "FK_TblStudent_TblClassRoom_ClassRoom_ID",
                        column: x => x.ClassRoom_ID,
                        principalTable: "TblClassRoom",
                        principalColumn: "ClassRoom_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblClassRoom_Class_ID",
                table: "TblClassRoom",
                column: "Class_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TblStudent_ClassRoom_ID",
                table: "TblStudent",
                column: "ClassRoom_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblStudent");

            migrationBuilder.DropTable(
                name: "TblClassRoom");

            migrationBuilder.DropTable(
                name: "TblClass");

            migrationBuilder.CreateTable(
                name: "Grade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeNumber = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClassNumber = table.Column<int>(type: "int", nullable: false),
                    ClassTeacher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Class_Grade_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Student_Grade_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Class_GradeId",
                table: "Class",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_ClassId",
                table: "Student",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_GradeId",
                table: "Student",
                column: "GradeId");
        }
    }
}
