using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    public class TblClassRoom
    {
        [Key]
        public int ClassRoom_ID { get; set; }

        [ForeignKey("Class")]
        public int Class_ID { get; set; }

        public string ClassRoom_Name { get; set; }
        public string? ClassRoom_Visible { get; set; }
        public string? ClassRoom_AddUserID { get; set; }
        public DateTime? ClassRoom_AddDate { get; set; }
        public string? ClassRoom_EditUserID { get; set; }
        public DateTime? ClassRoom_EditDate { get; set; }
        public string? ClassRoom_DeleteUserID { get; set; }
        public DateTime? ClassRoom_DeleteDate { get; set; }

        // Navigation properties
        public TblClass? Class { get; set; }
        public ICollection<TblStudent>? Students { get; set; }
    }
}
