using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.Core.Entities
{
    public class Student : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string StudentNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        [Required]
        [StringLength(15)]
        [Phone]
        public string PhoneNumber { get; set; }

        // Foreign Keys
        [Required]
        public int GradeId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public string QRCode { get; set; }

        public bool IsVisible { get; set; } = true;

        // Audit Fields
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        // Navigation Properties
        public virtual Grade Grade { get; set; }
        public virtual Class Class { get; set; }
    }
}
