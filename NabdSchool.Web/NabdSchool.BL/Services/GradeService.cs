using NabdSchool.Core.Entities;
using NabdSchool.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.BL.Services
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Grade>> GetAllGradesAsync()
        {
            return await _unitOfWork.Grades.FindAsync(g => g.IsActive);
        }

        public async Task<IEnumerable<Class>> GetClassesByGradeIdAsync(int gradeId)
        {
            return await _unitOfWork.Classes.FindAsync(c => c.GradeId == gradeId && c.IsActive);
        }

        public async Task<Grade> GetGradeByIdAsync(int id)
        {
            return await _unitOfWork.Grades.GetByIdAsync(id);
        }

        public async Task<Class> GetClassByIdAsync(int id)
        {
            return await _unitOfWork.Classes.GetByIdAsync(id);
        }
    }
}
