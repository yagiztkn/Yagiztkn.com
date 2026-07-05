using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yago.Business.Abstract;
using Yago.Core.Entities;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimelineController : ControllerBase
    {
        private readonly IGenericService<TimelineEntry> _timelineService;

        public TimelineController(IGenericService<TimelineEntry> timelineService)
        {
            _timelineService = timelineService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var entries = _timelineService.TGetList().OrderBy(t => t.DisplayOrder);
            return Ok(entries);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] TimelineEntry entry)
        {
            _timelineService.TInsert(entry);
            return Ok(entry);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] TimelineEntry updated)
        {
            var existing = _timelineService.TGetByID(id);
            if (existing == null) return NotFound();

            existing.Year = updated.Year;
            existing.Title = updated.Title;
            existing.Description = updated.Description;

            _timelineService.TUpdate(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var entry = _timelineService.TGetByID(id);
            if (entry == null) return NotFound();

            _timelineService.TDelete(entry);
            return Ok();
        }

        [HttpPost("reorder")]
        [Authorize]
        public IActionResult Reorder([FromBody] List<int> orderedIds)
        {
            var entries = _timelineService.TGetList();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var entry = entries.FirstOrDefault(t => t.Id == orderedIds[i]);
                if (entry != null)
                {
                    entry.DisplayOrder = i;
                    _timelineService.TUpdate(entry);
                }
            }
            return Ok();
        }
    }
}