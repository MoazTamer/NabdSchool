using NabdSchool.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.BL.Services
{
    public interface IGradeService
    {
        Task<IEnumerable<Grade>> GetAllGradesAsync();
        Task<IEnumerable<Class>> GetClassesByGradeIdAsync(int gradeId);
        Task<Grade> GetGradeByIdAsync(int id);
        Task<Class> GetClassByIdAsync(int id);
    }
}
