using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.Core.Entities
{
    public class Grade : BaseEntity
    {
        [Required]
        [Range(1, 12)]
        public int GradeNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string GradeName { get; set; } 

        [StringLength(50)]
        public string Stage { get; set; } 

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public virtual ICollection<Class> Classes { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
