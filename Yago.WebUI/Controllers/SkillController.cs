using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yago.Business.Abstract;
using Yago.Core.Entities;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly IGenericService<Skill> _skillService;

        public SkillController(IGenericService<Skill> skillService)
        {
            _skillService = skillService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var skills = _skillService.TGetList().OrderBy(s => s.DisplayOrder);
            return Ok(skills);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] Skill skill)
        {
            _skillService.TInsert(skill);
            return Ok(skill);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] Skill updated)
        {
            var existing = _skillService.TGetByID(id);
            if (existing == null) return NotFound();

            existing.Category = updated.Category;
            existing.Name = updated.Name;

            _skillService.TUpdate(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var skill = _skillService.TGetByID(id);
            if (skill == null) return NotFound();

            _skillService.TDelete(skill);
            return Ok();
        }

        [HttpPost("reorder")]
        [Authorize]
        public IActionResult Reorder([FromBody] List<int> orderedIds)
        {
            var skills = _skillService.TGetList();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var skill = skills.FirstOrDefault(s => s.Id == orderedIds[i]);
                if (skill != null)
                {
                    skill.DisplayOrder = i;
                    _skillService.TUpdate(skill);
                }
            }
            return Ok();
        }
    }
}