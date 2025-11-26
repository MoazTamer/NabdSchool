using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesModel.Models
{
    [Table("TblStudentBadges")]
    public class StudentBadge
    {
        [Key]
        public int Badge_ID { get; set; }

        [Required]
        public int Student_ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Badge_Type { get; set; } // نوع الشارة: انضباط، حضور_متتالي

        [Required]
        [StringLength(50)]
        public string Badge_Level { get; set; } // مستوى الشارة: برونزي، فضي، ذهبي، ماسي

        [Required]
        public int Points { get; set; } // النقاط المكتسبة

        [Required]
        public DateTime Earned_Date { get; set; } = DateTime.Now; // تاريخ الحصول على الشارة

        [StringLength(10)]
        public string Badge_Visible { get; set; } = "yes";

        // Navigation Property
        [ForeignKey("Student_ID")]
        public virtual TblStudent Student { get; set; }
    }

    [Table("TblStudentPoints")]
    public class StudentPoints
    {
        [Key]
        public int Point_ID { get; set; }

        [Required]
        public int Student_ID { get; set; }

        [Required]
        public int Total_Points { get; set; } = 0; // إجمالي النقاط

        [Required]
        public int Monthly_Points { get; set; } = 0; // نقاط الشهر الحالي

        [Required]
        public int Attendance_Streak { get; set; } = 0; // عدد أيام الحضور المتتالية

        [Required]
        public DateTime Last_Updated { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("Student_ID")]
        public virtual TblStudent Student { get; set; }
    }

    [Table("TblBadgeDefinitions")]
    public class BadgeDefinition
    {
        [Key]
        public int Definition_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Badge_Name { get; set; } // اسم الشارة

        [Required]
        [StringLength(50)]
        public string Badge_Type { get; set; } // نوع الشارة: انضباط، حضور_متتالي

        [Required]
        [StringLength(50)]
        public string Badge_Level { get; set; } // المستوى: برونزي، فضي، ذهبي، ماسي

        [Required]
        public int Required_Points { get; set; } // النقاط المطلوبة

        [Required]
        [StringLength(50)]
        public string Badge_Icon { get; set; } // أيقونة الشارة

        [Required]
        [StringLength(20)]
        public string Badge_Color { get; set; } // لون الشارة

        [StringLength(500)]
        public string Description { get; set; } // وصف الشارة

        [Required]
        public bool Is_Active { get; set; } = true;

        public DateTime Created_Date { get; set; } = DateTime.Now;
    }

    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int Disciplined { get; set; }
        public string Period { get; set; } = "day";
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();

        // بيانات الشارات
        public List<TopStudentBadge> TopStudents { get; set; } = new List<TopStudentBadge>();
        public BadgeStatistics BadgeStats { get; set; } = new BadgeStatistics();
    }

    public class TopStudentBadge
    {
        public int Student_ID { get; set; }
        public string Student_Name { get; set; }
        public int Total_Points { get; set; }
        public int Attendance_Streak { get; set; }
        public List<StudentBadgeInfo> Badges { get; set; } = new List<StudentBadgeInfo>();
        public string HighestBadgeLevel { get; set; }
        public string BadgeColor { get; set; }
    }

    public class StudentBadgeInfo
    {
        public string Badge_Name { get; set; }
        public string Badge_Level { get; set; }
        public string Badge_Icon { get; set; }
        public string Badge_Color { get; set; }
        public int Points { get; set; }
    }

    public class BadgeStatistics
    {
        public int TotalBadgesEarned { get; set; }
        public int DiamondBadges { get; set; }
        public int GoldBadges { get; set; }
        public int SilverBadges { get; set; }
        public int BronzeBadges { get; set; }
    }
}

