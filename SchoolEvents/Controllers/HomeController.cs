using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;

namespace SchoolEvents.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var upcoming = await _db.Schoolevents
                .Where(e => e.StartAt >= now)
                .OrderBy(e => e.StartAt)
                .Take(6)
                .ToListAsync();

            return View(upcoming);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ CONTACT (GET)
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // ✅ CONTACT (POST) -> запис в DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string subject, string message)
        {
            var msg = new ContactMessage
            {
                Name = name,
                Email = email,
                Subject = subject,
                Message = message,
                CreatedAt = DateTime.Now
            };

            _db.ContactMessages.Add(msg);
            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] = "Благодарим! Съобщението е изпратено успешно.";
            return RedirectToAction(nameof(Contact));
        }
    }
}
