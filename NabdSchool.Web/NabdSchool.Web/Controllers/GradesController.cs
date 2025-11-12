using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NabdSchool.BL.Services;

namespace NabdSchool.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        // GET: api/grades
        [HttpGet]
        public async Task<IActionResult>
    GetAllGrades()
        {
            var grades = await _gradeService.GetAllGradesAsync();
            var result = grades.Select(g => new
            {
                id = g.Id,
                gradeName = g.GradeName,
                gradeNumber = g.GradeNumber,
                stage = g.Stage
            });
            return Ok(result);
        }

        // GET: api/grades/5/classes
        [HttpGet("{gradeId}/classes")]
        public async Task<IActionResult>
            GetClassesByGrade(int gradeId)
        {
            var classes = await _gradeService.GetClassesByGradeIdAsync(gradeId);
            var result = classes.Select(c => new
            {
                id = c.Id,
                className = c.ClassName,
                classNumber = c.ClassNumber,
                capacity = c.Capacity,
                classTeacher = c.ClassTeacher
            });
            return Ok(result);
        }
    }
}