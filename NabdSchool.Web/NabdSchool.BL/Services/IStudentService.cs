using NabdSchool.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.BL.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync(bool includeDeleted = false);
        Task<Student> GetStudentByIdAsync(int id);
        Task<Student> CreateStudentAsync(Student student, string createdBy);
        Task<bool> UpdateStudentAsync(Student student, string modifiedBy);
        Task<bool> SoftDeleteStudentAsync(int id, string deletedBy);
        Task<bool> RestoreStudentAsync(int id, string restoredBy);
        Task<(bool Success, string Message, int ImportedCount)> ImportFromExcelAsync(Stream fileStream, string importedBy);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(int grade, bool includeDeleted = false);
        Task<IEnumerable<Student>> GetStudentsByClassAsync(int grade, int classNumber, bool includeDeleted = false);
        Task<bool> StudentNumberExistsAsync(string studentNumber, int? excludeId = null);
    }

}
