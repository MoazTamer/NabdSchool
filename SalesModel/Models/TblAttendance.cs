using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesModel.Models
{
    public class TblAttendance
    {
        [Key]
        public int Attendance_ID { get; set; }

        [ForeignKey("Student")]
        public int Student_ID { get; set; }

        public DateTime Attendance_Date { get; set; }
        public TimeSpan Attendance_Time { get; set; }
        public int Attendance_LateMinutes { get; set; }
        public string? Attendance_Status { get; set; }
        public string? Attendance_Notes { get; set; }
        public string? Attendance_Visible { get; set; }
        public string? Attendance_AddUserID { get; set; }
        public DateTime? Attendance_AddDate { get; set; }
        public string? Attendance_EditUserID { get; set; }
        public DateTime? Attendance_EditDate { get; set; }
        public string? Attendance_DeleteUserID { get; set; }
        public DateTime? Attendance_DeleteDate { get; set; }

        // Navigation property
        public TblStudent? Student { get; set; }
    }
}