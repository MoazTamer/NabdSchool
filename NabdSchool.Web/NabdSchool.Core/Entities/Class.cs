using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.Core.Entities
{
    public class Class : BaseEntity
    {
        [Required]
        public int GradeId { get; set; } // Foreign Key

        [Required]
        [Range(1, 50)]
        public int ClassNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } 

        [StringLength(200)]
        public string? ClassTeacher { get; set; } 

        [Range(1, 100)]
        public int Capacity { get; set; } 
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Grade Grade { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
