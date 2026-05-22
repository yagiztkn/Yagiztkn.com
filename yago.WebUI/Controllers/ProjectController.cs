using Microsoft.AspNetCore.Mvc;
using System;
using Yago.Business.Abstract;
using Yago.Core.Entities;

namespace Yago.WebUI.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IGenericService<Project> _projectService;

        public ProjectController(IGenericService<Project> projectService)
        {
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            var projects = _projectService.TGetList();
            return View(projects);
        }

        // YENİ EKLEDİĞİMİZ DETAY METODU
        public IActionResult Details(int id)
        {
            // Servis üzerinden ID'ye göre projeyi getiriyoruz
            var project = _projectService.TGetByID(id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProject(Project project)
        {
            project.CreatedDate = DateTime.Now;

            // Eğer formdan gelmiyorsa varsayılan değerleri burada atayabilirsin
            if (string.IsNullOrEmpty(project.FullDescription))
                project.FullDescription = "Bu Projenin Detaylı Açıklamasıdır...";

            _projectService.TInsert(project);
            return RedirectToAction("Index");
        }
    }
}
