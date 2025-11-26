using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesModel.Models;

namespace SalesRepository.Data
{
	public class SalesDBContext : IdentityDbContext
	{
		public SalesDBContext(DbContextOptions<SalesDBContext> options)
			: base(options)
		{
		}

		public DbSet<ApplicationUser> ApplicationUser { get; set; }
		public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<TblBranch> TblBranch { get; set; }
		public DbSet<TblVat> TblVat { get; set; }
        
		public DbSet<TblProduct_Category> TblProduct_Category { get; set; }
		public DbSet<TblStudent> TblStudent { get; set; }
        public DbSet<TblClass> TblClass { get; set; }
		public DbSet<TblClassRoom> TblClassRoom { get; set; }
		public DbSet<TblSchoolSettings> TblSchoolSettings { get; set; }
		public DbSet<TblAttendance> TblAttendance { get; set; }
        public DbSet<StudentBadge> TblStudentBadges { get; set; }
        public DbSet<StudentPoints> TblStudentPoints { get; set; }
        public DbSet<BadgeDefinition> TblBadgeDefinitions { get; set; }

        public DbSet<AuditLog> AuditLog { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⭐ تكوين العلاقات للشارات

            // علاقة StudentBadge مع TblStudent
            modelBuilder.Entity<StudentBadge>()
                .HasOne(sb => sb.Student)
                .WithMany()
                .HasForeignKey(sb => sb.Student_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة StudentPoints مع TblStudent
            modelBuilder.Entity<StudentPoints>()
                .HasOne(sp => sp.Student)
                .WithMany()
                .HasForeignKey(sp => sp.Student_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // إنشاء Indexes لتحسين الأداء
            modelBuilder.Entity<StudentPoints>()
                .HasIndex(sp => sp.Student_ID);

            modelBuilder.Entity<StudentPoints>()
                .HasIndex(sp => sp.Total_Points);

            modelBuilder.Entity<StudentBadge>()
                .HasIndex(sb => sb.Student_ID);

            modelBuilder.Entity<StudentBadge>()
                .HasIndex(sb => new { sb.Badge_Type, sb.Badge_Level });

            SeedBadgeDefinitions(modelBuilder);
        }

        private void SeedBadgeDefinitions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BadgeDefinition>().HasData(
                new BadgeDefinition
                {
                    Definition_ID = 1,
                    Badge_Name = "المنضبطة",
                    Badge_Type = "انضباط",
                    Badge_Level = "برونزي",
                    Required_Points = 50,
                    Badge_Icon = "medal",
                    Badge_Color = "#CD7F32",
                    Description = "حضور منتظم لمدة أسبوعين"
                },
                new BadgeDefinition
                {
                    Definition_ID = 2,
                    Badge_Name = "المنضبطة",
                    Badge_Type = "انضباط",
                    Badge_Level = "فضي",
                    Required_Points = 150,
                    Badge_Icon = "medal",
                    Badge_Color = "#C0C0C0",
                    Description = "حضور منتظم لمدة شهر"
                },
                new BadgeDefinition
                {
                    Definition_ID = 3,
                    Badge_Name = "المنضبطة",
                    Badge_Type = "انضباط",
                    Badge_Level = "ذهبي",
                    Required_Points = 300,
                    Badge_Icon = "medal",
                    Badge_Color = "#FFD700",
                    Description = "حضور منتظم لمدة شهرين"
                },
                new BadgeDefinition
                {
                    Definition_ID = 4,
                    Badge_Name = "المنضبطة",
                    Badge_Type = "انضباط",
                    Badge_Level = "ماسي",
                    Required_Points = 500,
                    Badge_Icon = "medal",
                    Badge_Color = "#B9F2FF",
                    Description = "حضور منتظم لمدة فصل كامل"
                },

                // شارات الحضور المتتالي
                new BadgeDefinition
                {
                    Definition_ID = 5,
                    Badge_Name = "المواظبة",
                    Badge_Type = "حضور_متتالي",
                    Badge_Level = "برونزي",
                    Required_Points = 30,
                    Badge_Icon = "chart-simple",
                    Badge_Color = "#CD7F32",
                    Description = "7 أيام حضور متتالي"
                },
                new BadgeDefinition
                {
                    Definition_ID = 6,
                    Badge_Name = "المواظبة",
                    Badge_Type = "حضور_متتالي",
                    Badge_Level = "فضي",
                    Required_Points = 70,
                    Badge_Icon = "chart-simple",
                    Badge_Color = "#C0C0C0",
                    Description = "14 يوم حضور متتالي"
                },
                new BadgeDefinition
                {
                    Definition_ID = 7,
                    Badge_Name = "المواظبة",
                    Badge_Type = "حضور_متتالي",
                    Badge_Level = "ذهبي",
                    Required_Points = 150,
                    Badge_Icon = "chart-simple",
                    Badge_Color = "#FFD700",
                    Description = "30 يوم حضور متتالي"
                },
                new BadgeDefinition
                {
                    Definition_ID = 8,
                    Badge_Name = "المواظبة",
                    Badge_Type = "حضور_متتالي",
                    Badge_Level = "ماسي",
                    Required_Points = 300,
                    Badge_Icon = "chart-simple",
                    Badge_Color = "#B9F2FF",
                    Description = "60 يوم حضور متتالي"
                }
            );
        }
    }
}
