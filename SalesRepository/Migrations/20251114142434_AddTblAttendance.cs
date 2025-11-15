using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesRepository.Migrations
{
    /// <inheritdoc />
    public partial class AddTblAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TblAttendance",
                columns: table => new
                {
                    Attendance_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<int>(type: "int", nullable: false),
                    Attendance_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attendance_Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Attendance_LateMinutes = table.Column<int>(type: "int", nullable: false),
                    Attendance_Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_Visible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_AddUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_AddDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attendance_EditUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attendance_DeleteUserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance_DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblAttendance", x => x.Attendance_ID);
                    table.ForeignKey(
                        name: "FK_TblAttendance_TblStudent_Student_ID",
                        column: x => x.Student_ID,
                        principalTable: "TblStudent",
                        principalColumn: "Student_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblAttendance_Student_ID",
                table: "TblAttendance",
                column: "Student_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblAttendance");
        }
    }
}
