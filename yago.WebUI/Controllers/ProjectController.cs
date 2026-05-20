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

        [HttpGet]
        public IActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProject(Project project)
        {
           project.CreatedDate = DateTime.Now;

            project.FullDescription = "Bu Projenin Detaylı Açıklamasıdır. Projenin Amacı, Kullanılan Teknolojiler ve Diğer Bilgiler Buraya Yazılabilir.";

            project.LiveLink = "Yayında değil";

            project.ShortDescription = "Kısa açıklama eklenecek.";

            _projectService.TInsert(project);

            return RedirectToAction("Index");
        }
    }   
}
