using NabdSchool.Core.Entities;
using NabdSchool.Core.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.BL.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync(bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                return await _unitOfWork.Students.GetAllAsync();
            }
            return await _unitOfWork.Students.FindAsync(s => s.IsVisible);
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _unitOfWork.Students.GetByIdAsync(id);
        }

        public async Task<Student> CreateStudentAsync(Student student, string createdBy)
        {
            student.CreatedBy = createdBy;
            student.CreatedDate = DateTime.Now;
            student.IsVisible = true;

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            return student;
        }

        public async Task<bool> UpdateStudentAsync(Student student, string modifiedBy)
        {
            var existingStudent = await _unitOfWork.Students.GetByIdAsync(student.Id);
            if (existingStudent == null)
                return false;

            existingStudent.FullName = student.FullName;
            existingStudent.PhoneNumber = student.PhoneNumber;
            existingStudent.GradeId = student.GradeId;
            existingStudent.ClassId = student.ClassId;
            existingStudent.ModifiedBy = modifiedBy;
            existingStudent.ModifiedDate = DateTime.Now;

            await _unitOfWork.Students.UpdateAsync(existingStudent);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SoftDeleteStudentAsync(int id, string deletedBy)
        {
            await _unitOfWork.Students.SoftDeleteAsync(id, deletedBy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreStudentAsync(int id, string restoredBy)
        {
            await _unitOfWork.Students.RestoreAsync(id, restoredBy);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(int gradeId, bool includeDeleted = false)
        {
            if (includeDeleted)
                return await _unitOfWork.Students.FindAsync(s => s.GradeId == gradeId);

            return await _unitOfWork.Students.FindAsync(s => s.GradeId == gradeId && s.IsVisible);
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassAsync(int gradeId, int classId, bool includeDeleted = false)
        {
            if (includeDeleted)
                return await _unitOfWork.Students.FindAsync(s => s.GradeId == gradeId && s.ClassId == classId);

            return await _unitOfWork.Students.FindAsync(s => s.GradeId == gradeId && s.ClassId == classId && s.IsVisible);
        }

        public async Task<bool> StudentNumberExistsAsync(string studentNumber, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _unitOfWork.Students.ExistsAsync(s => s.StudentNumber == studentNumber && s.Id != excludeId.Value);
            }
            return await _unitOfWork.Students.ExistsAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string importedBy)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet.Dimension == null)
                    return (false, "الملف فارغ", 0);

                var students = new List<Student>();
                var rowCount = worksheet.Dimension.Rows;
                var errors = new List<string>();

                // البدء من الصف الثاني (تخطي الهيدر)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var studentNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var fullName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var gradeStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var classNumberStr = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var phoneNumber = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                        // التحقق من البيانات الأساسية
                        if (string.IsNullOrEmpty(studentNumber) || string.IsNullOrEmpty(fullName))
                        {
                            errors.Add($"الصف {row}: بيانات غير مكتملة");
                            continue;
                        }

                        // التحقق من تكرار رقم الطالب
                        if (await StudentNumberExistsAsync(studentNumber))
                        {
                            errors.Add($"الصف {row}: رقم الطالب {studentNumber} موجود مسبقاً");
                            continue;
                        }

                        // استخراج رقم الصف من النص (مثلاً: "0723" -> Grade = 7, Class = 23)
                        int grade = 0;
                        int classNumber = 0;

                        if (!string.IsNullOrEmpty(gradeStr) && gradeStr.Length >= 2)
                        {
                            // استخراج أول رقمين للصف
                            int.TryParse(gradeStr.Substring(0, 2), out grade);

                            // استخراج باقي الأرقام للفصل
                            if (gradeStr.Length > 2)
                            {
                                int.TryParse(gradeStr.Substring(2), out classNumber);
                            }
                        }

                        if (!string.IsNullOrEmpty(classNumberStr))
                        {
                            int.TryParse(classNumberStr, out classNumber);
                        }

                        if (grade == 0 || classNumber == 0)
                        {
                            errors.Add($"الصف {row}: رقم الصف أو الفصل غير صحيح");
                            continue;
                        }

                        var student = new Student
                        {
                            StudentNumber = studentNumber,
                            FullName = fullName,
                            GradeId = grade,
                            ClassId = classNumber,
                            PhoneNumber = phoneNumber ?? "",
                            IsVisible = true,
                            CreatedBy = importedBy,
                            CreatedDate = DateTime.Now
                        };

                        students.Add(student);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"الصف {row}: خطأ - {ex.Message}");
                    }
                }

                if (students.Any())
                {
                    await _unitOfWork.Students.AddRangeAsync(students);
                    await _unitOfWork.SaveChangesAsync();
                }

                var message = $"تم استيراد {students.Count} طالب بنجاح";
                if (errors.Any())
                {
                    message += $"\n\nأخطاء ({errors.Count}):\n" + string.Join("\n", errors.Take(10));
                    if (errors.Count > 10)
                        message += $"\n... و {errors.Count - 10} خطأ آخر";
                }

                return (true, message, students.Count);
            }
            catch (Exception ex)
            {
                return (false, $"خطأ في معالجة الملف: {ex.Message}", 0);
            }
        }
    }
}
