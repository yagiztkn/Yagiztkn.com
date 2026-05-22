using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Yago.Core.Entities;
using Yago.DataAcsess.Context;

namespace yago.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {

        var featuredProjects = _context.Projects
        .OrderByDescending(x => x.CreatedDate)
        .Take(3)
        .ToList();

        return View(featuredProjects);
    }

    [HttpPost]
    public IActionResult SendMessage(string FullName, string Email, string Subject, string Message)
    {
        // Yeni bir mesaj nesnesi oluşturuyoruz ve gelen verileri içine tek tek koyuyoruz
        var contactMessage = new ContactMessage
        {
            FullName = FullName,
            Email = Email,
            Subject = Subject,
            Message = Message,
            SendDate = DateTime.Now,
            IsRead = false
        };

        // Veritabanına ekliyoruz
        _context.ContactMessages.Add(contactMessage);
        _context.SaveChanges();

        // Başarılı! Ana sayfaya dön.
        return RedirectToAction("Index");
    }
}