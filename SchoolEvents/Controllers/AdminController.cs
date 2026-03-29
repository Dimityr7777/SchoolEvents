using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;

namespace SchoolEvents.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // /Admin
        [HttpGet("/Admin")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var model = new AdminDashboardViewModel
            {
                TotalEvents = await _db.Schoolevents.CountAsync(),
                UpcomingEvents = await _db.Schoolevents.CountAsync(e => e.StartAt.HasValue && e.StartAt.Value.Date >= today),
                PastEvents = await _db.Schoolevents.CountAsync(e => e.StartAt.HasValue && e.StartAt.Value.Date < today),
                TotalNews = await _db.NewsPosts.CountAsync(),
                PublishedNews = await _db.NewsPosts.CountAsync(n => n.IsPublished),
                TotalMessages = await _db.ContactMessages.CountAsync(),
                TotalAlbums = await _db.GalleryAlbums.CountAsync(),
                TotalPhotos = await _db.GalleryPhotos.CountAsync()
            };

            return View(model);
        }

        // /Admin/Messages
        public async Task<IActionResult> Messages()
        {
            var msgs = await _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(msgs);
        }

        // /Admin/Events
        public async Task<IActionResult> Events()
        {
            var eventsList = await _db.Schoolevents
                .OrderBy(e => e.StartAt.HasValue ? 0 : 1)
                .ThenByDescending(e => e.StartAt)
                .ToListAsync();

            return View(eventsList);
        }

        // /Admin/EditEvent/5 (GET)
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var ev = await _db.Schoolevents.FindAsync(id);
            if (ev == null) return NotFound();

            return View(ev);
        }

        // /Admin/EditEvent/5 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Schoolevent model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var ev = await _db.Schoolevents.FindAsync(id);
            if (ev == null) return NotFound();

            ev.Title = model.Title;
            ev.Description = model.Description;
            ev.StartAt = model.StartAt.HasValue ? model.StartAt.Value.Date : null;
            ev.Location = model.Location;
            ev.Type = model.Type;

            await _db.SaveChangesAsync();

            TempData["AdminSuccess"] = "Събитието е обновено успешно.";
            return RedirectToAction(nameof(Events));
        }

        // /Admin/DeleteEvent/5 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _db.Schoolevents.FindAsync(id);
            if (ev == null) return NotFound();

            _db.Schoolevents.Remove(ev);
            await _db.SaveChangesAsync();

            TempData["AdminSuccess"] = "Събитието е изтрито успешно.";
            return RedirectToAction(nameof(Events));
        }
    }
}