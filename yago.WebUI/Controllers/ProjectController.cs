using Microsoft.AspNetCore.Mvc;
using System;
using Yago.Business.Abstract;
using Yago.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Yago.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // -> /api/Project
    public class ProjectController : ControllerBase   // DEĞİŞTİ: Controller -> ControllerBase
    {
        private readonly IGenericService<Project> _projectService;

        public ProjectController(IGenericService<Project> projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var projects = _projectService.TGetList()
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.CreatedDate);

            return Ok(projects);
        }

        [HttpGet("{id}")]          // GET /api/Project/5
        public IActionResult GetById(int id)
        {
            var project = _projectService.TGetByID(id);

            if (project == null)
                return NotFound();

            return Ok(project);    // DEĞİŞTİ: View(project) -> Ok(project)
        }

        [HttpPost]                 // POST /api/Project
        [Authorize]
        public IActionResult Create([FromBody] Project project)   // DEĞİŞTİ: [FromBody] eklendi
        {
            project.CreatedDate = DateTime.Now;

            if (string.IsNullOrEmpty(project.FullDescription))
                project.FullDescription = "Bu Projenin Detaylı Açıklamasıdır...";

            _projectService.TInsert(project);

            // DEĞİŞTİ: RedirectToAction("Index") -> CreatedAtAction
            return CreatedAtAction(nameof(GetById), new { id = project.ID }, project);
        }

        [HttpPatch("{id}/featured")]
        [Authorize]
        public IActionResult ToggleFeatured(int id)
        {
            var project = _projectService.TGetByID(id);
            if (project == null) return NotFound();

            project.IsFeatured = !project.IsFeatured;
            _projectService.TUpdate(project);

            return Ok(project);
        }

        [HttpPost("reorder")]
        [Authorize]
        public IActionResult Reorder([FromBody] List<int> orderedIds)
        {
            var projects = _projectService.TGetList();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                var project = projects.FirstOrDefault(p => p.ID == orderedIds[i]);
                if (project != null)
                {
                    project.DisplayOrder = i;
                    _projectService.TUpdate(project);
                }
            }

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] Project updatedProject)
        {
            var existing = _projectService.TGetByID(id);
            if (existing == null) return NotFound();

            existing.Title = updatedProject.Title;
            existing.ShortDescription = updatedProject.ShortDescription;
            existing.FullDescription = updatedProject.FullDescription ?? "";
            existing.Technologies = updatedProject.Technologies;
            existing.GitHubLink = updatedProject.GitHubLink ?? "";
            existing.LiveLink = updatedProject.LiveLink ?? "";

            _projectService.TUpdate(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var project = _projectService.TGetByID(id);
            if (project == null) return NotFound();

            _projectService.TDelete(project);
            return Ok();
        }

        [HttpGet("featured")]
        public IActionResult GetFeatured()
        {
            var allProjects = _projectService.TGetList();

            var featured = allProjects
                .Where(p => p.IsFeatured)
                .OrderBy(p => p.DisplayOrder)
                .Take(3)
                .ToList();

            // Hiç öne çıkan proje işaretlenmemişse, en son eklenen 3 projeyi göster
            if (!featured.Any())
            {
                featured = allProjects
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(3)
                    .ToList();
            }

            return Ok(featured);
        }
    }
}
