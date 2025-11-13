using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    public class TblStudent
    {
        [Key]
        public int Student_ID { get; set; }

        [ForeignKey("ClassRoom")]
        public int ClassRoom_ID { get; set; }

        public string Student_Name { get; set; }
        public string? Student_Code { get; set; } 
        public string? Student_Phone { get; set; } 
        public string? Student_Address { get; set; }
        public DateTime? Student_BirthDate { get; set; }
        public string? Student_Gender { get; set; } 
        public string? Student_Notes { get; set; }
        public string? Student_Visible { get; set; }
        public string? Student_AddUserID { get; set; }
        public DateTime? Student_AddDate { get; set; }
        public string? Student_EditUserID { get; set; }
        public DateTime? Student_EditDate { get; set; }
        public string? Student_DeleteUserID { get; set; }
        public DateTime? Student_DeleteDate { get; set; }

        // Navigation properties
        public TblClassRoom? ClassRoom { get; set; }
    }
}
