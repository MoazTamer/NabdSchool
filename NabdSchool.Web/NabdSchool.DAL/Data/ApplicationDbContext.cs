using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NabdSchool.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NabdSchool.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Grade Configuration
            builder.Entity<Grade>(entity =>
            {
                entity.HasIndex(e => e.GradeNumber).IsUnique();
                entity.HasMany(e => e.Classes)
                      .WithOne(e => e.Grade)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Class Configuration
            builder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => new { e.GradeId, e.ClassNumber }).IsUnique();
                entity.HasMany(e => e.Students)
                      .WithOne(e => e.Class)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Student Configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.StudentNumber).IsUnique();
                entity.HasIndex(e => e.IsVisible);

                entity.HasOne(e => e.Grade)
                      .WithMany(e => e.Students)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Initial Data
            SeedGradesAndClasses(builder);
            //SeedStudents(builder);
        }

        private void SeedGradesAndClasses(ModelBuilder builder)
        {
            // 3 Grades (متوسط فقط)
            var grades = new List<Grade>
            {
                new Grade { Id = 1, GradeNumber = 7, GradeName = "الصف الأول المتوسط", Stage="متوسط", IsActive=true },
                new Grade { Id = 2, GradeNumber = 8, GradeName = "الصف الثاني المتوسط", Stage="متوسط", IsActive=true },
                new Grade { Id = 3, GradeNumber = 9, GradeName = "الصف الثالث المتوسط", Stage="متوسط", IsActive=true }
            };
            builder.Entity<Grade>().HasData(grades);

            // 5 Classes per grade
            var classes = new List<Class>();
            int classId = 1;
            foreach (var grade in grades)
            {
                for (int i = 1; i <= 5; i++)
                {
                    classes.Add(new Class
                    {
                        Id = classId++,
                        GradeId = grade.Id,
                        ClassNumber = i,
                        ClassName = $"الفصل {i}",
                        Capacity = 30,
                        IsActive = true
                    });
                }
            }
            builder.Entity<Class>().HasData(classes);
        }

        //private void SeedStudents(ModelBuilder builder)
        //{
        //    var students = new List<Student>
        //    {
        //        new Student { Id=1, StudentNumber="0723", FullName="طالب 1", GradeId=1, ClassId=1, IsVisible=true },
        //        new Student { Id=2, StudentNumber="0825", FullName="طالب 2", GradeId=2, ClassId=1, IsVisible=true },
        //        new Student { Id=3, StudentNumber="0925", FullName="طالب 3", GradeId=3, ClassId=1, IsVisible=true }
        //    };

        //    builder.Entity<Student>().HasData(students);
        //}

        private string GetArabicNumber(int number)
        {
            string[] arabicNumbers = { "", "الأول", "الثاني", "الثالث", "الرابع", "الخامس",
                                      "السادس", "السابع", "الثامن", "التاسع", "العاشر",
                                      "الحادي عشر", "الثاني عشر" };
            return number <= arabicNumbers.Length ? arabicNumbers[number] : number.ToString();
        }
    }

    public class AuditEntry
    {
        public string TableName { get; set; }
        public string Action { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, object> KeyValues { get; set; } = new();
        public Dictionary<string, object> OldValues { get; set; } = new();
        public Dictionary<string, object> NewValues { get; set; } = new();
        public List<PropertyEntry> TemporaryProperties { get; set; } = new();

        public AuditLog ToAuditLog()
        {
            return new AuditLog
            {
                TableName = TableName,
                Action = Action,
                UserId = UserId,
                UserName = UserName,
                DateTime = DateTime,
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues)
            };
        }
    }
}
