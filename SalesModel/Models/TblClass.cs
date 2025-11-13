using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    public class TblClass
    {
        [Key]
        public int Class_ID { get; set; }
        public string Class_Name { get; set; }
        public string? Class_Visible { get; set; }
        public string? Class_AddUserID { get; set; }
        public DateTime? Class_AddDate { get; set; }
        public string? Class_EditUserID { get; set; }
        public DateTime? Class_EditDate { get; set; }
        public string? Class_DeleteUserID { get; set; }
        public DateTime? Class_DeleteDate { get; set; }

        // Navigation property
        public ICollection<TblClassRoom>? ClassRooms { get; set; }
    }
}
