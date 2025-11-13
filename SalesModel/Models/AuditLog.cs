using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    public class AuditLog 
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TableName { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string UserName { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        public string IPAddress { get; set; }
    }
}
