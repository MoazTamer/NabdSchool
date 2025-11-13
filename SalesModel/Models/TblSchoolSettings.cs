using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    [Table("TblSchoolSettings")]
    public class TblSchoolSettings
    {
        [Key]
        public int Setting_ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Setting_Key { get; set; } // مثل "AttendanceTime"

        [StringLength(100)]
        public string Setting_Value { get; set; } // مثل "07:30"

        [StringLength(200)]
        public string Setting_Description { get; set; }

        public string Setting_Visible { get; set; } = "yes";

        public string Setting_AddUserID { get; set; }
        public DateTime? Setting_AddDate { get; set; }

        public string? Setting_EditUserID { get; set; }
        public DateTime? Setting_EditDate { get; set; }

        public string? Setting_DeleteUserID { get; set; }
        public DateTime? Setting_DeleteDate { get; set; }
    }
}
