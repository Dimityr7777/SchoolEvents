using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;
using SchoolEvents.Services;

namespace SchoolEvents.Controllers
{
    [Authorize]
    public class SchooleventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public SchooleventsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _environment = environment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? filter, string? q)
        {
            var today = DateTime.Today;
            IQueryable<Schoolevent> query = _context.Schoolevents.AsNoTracking();

            if (filter == "past")
            {
                query = query.Where(e => e.StartAt.HasValue && e.StartAt.Value.Date < today);
            }
            else if (filter == "all")
            {
                filter = "all";
            }
            else
            {
                query = query.Where(e => e.StartAt.HasValue && e.StartAt.Value.Date >= today);
                filter = "upcoming";
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(e =>
                    e.Title.Contains(q) ||
                    (e.Description != null && e.Description.Contains(q)) ||
                    (e.Location != null && e.Location.Contains(q)) ||
                    (e.Type != null && e.Type.Contains(q))
                );
            }

            query = query.OrderByDescending(e => e.StartAt);

            ViewBag.Filter = filter;
            ViewBag.Q = q;

            return View(await query.ToListAsync());
        }

        [AllowAnonymous]
        public IActionResult Calendar()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Schoolevents
                .AsNoTracking()
                .Where(e => e.StartAt.HasValue)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartAt.HasValue ? e.StartAt.Value.ToString("yyyy-MM-dd") : null,
                    url = Url.Action("Details", "Schoolevents", new { id = e.Id }),
                    color = e.Type == "Спортно" ? "green" :
                            e.Type == "Културно" ? "purple" :
                            e.Type == "Официално" ? "red" :
                            e.Type == "Инициатива" ? "orange" :
                            "blue",
                    extendedProps = new
                    {
                        location = e.Location,
                        type = e.Type,
                        description = e.Description,
                        imagePath = e.ImagePath
                    }
                })
                .ToListAsync();

            return Json(events);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var schoolevent = await _context.Schoolevents
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (schoolevent == null) return NotFound();

            return View(schoolevent);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create(DateTime? startAt)
        {
            var model = new Schoolevent();

            if (startAt.HasValue)
                model.StartAt = startAt.Value.Date;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,StartAt,Location,Type,ImageFile")] Schoolevent schoolevent)
        {
            if (!ModelState.IsValid)
                return View(schoolevent);

            if (schoolevent.StartAt.HasValue)
            {
                schoolevent.StartAt = schoolevent.StartAt.Value.Date;
            }

            if (schoolevent.ImageFile != null && schoolevent.ImageFile.Length > 0)
            {
                schoolevent.ImagePath = await SaveImageAsync(schoolevent.ImageFile);
            }

            _context.Schoolevents.Add(schoolevent);
            await _context.SaveChangesAsync();

            var usersToNotify = _userManager.Users
                .Where(u => !string.IsNullOrEmpty(u.Email) && u.WantsEmailNotifications)
                .ToList();

            foreach (var user in usersToNotify)
            {
                try
                {
                    var subject = $"Ново събитие: {schoolevent.Title}";
                    var body = $@"
                        <h2>Ново училищно събитие</h2>
                        <p><strong>Заглавие:</strong> {schoolevent.Title}</p>
                        <p><strong>Описание:</strong> {schoolevent.Description}</p>
                        <p><strong>Дата:</strong> {(schoolevent.StartAt.HasValue ? schoolevent.StartAt.Value.ToString("dd.MM.yyyy") : "")}</p>
                        <p><strong>Място:</strong> {schoolevent.Location}</p>
                        <p><strong>Тип:</strong> {schoolevent.Type}</p>
                        <p><a href='https://localhost:7165/Schoolevents/Details/{schoolevent.Id}'>Виж събитието в сайта</a></p>";

                    await _emailService.SendEmailAsync(user.Email!, subject, body);
                }
                catch
                {
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var schoolevent = await _context.Schoolevents.FindAsync(id.Value);
            if (schoolevent == null) return NotFound();

            return View(schoolevent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartAt,Location,Type,ImagePath,ImageFile")] Schoolevent schoolevent)
        {
            if (id != schoolevent.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(schoolevent);

            var existingEvent = await _context.Schoolevents.FindAsync(id);
            if (existingEvent == null) return NotFound();

            existingEvent.Title = schoolevent.Title;
            existingEvent.Description = schoolevent.Description;
            existingEvent.StartAt = schoolevent.StartAt.HasValue ? schoolevent.StartAt.Value.Date : null;
            existingEvent.Location = schoolevent.Location;
            existingEvent.Type = schoolevent.Type;

            if (schoolevent.ImageFile != null && schoolevent.ImageFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(existingEvent.ImagePath))
                {
                    DeleteImageFile(existingEvent.ImagePath);
                }

                existingEvent.ImagePath = await SaveImageAsync(schoolevent.ImageFile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var schoolevent = await _context.Schoolevents
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            if (schoolevent == null) return NotFound();

            return View(schoolevent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolevent = await _context.Schoolevents.FindAsync(id);
            if (schoolevent == null) return RedirectToAction(nameof(Index));

            if (!string.IsNullOrWhiteSpace(schoolevent.ImagePath))
            {
                DeleteImageFile(schoolevent.ImagePath);
            }

            _context.Schoolevents.Remove(schoolevent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFromCalendar(int id)
        {
            var ev = await _context.Schoolevents.FindAsync(id);
            if (ev == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(ev.ImagePath))
            {
                DeleteImageFile(ev.ImagePath);
            }

            _context.Schoolevents.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "events");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/uploads/events/{uniqueFileName}";
        }

        private void DeleteImageFile(string imagePath)
        {
            var trimmedPath = imagePath.TrimStart('/');
            var fullPath = Path.Combine(_environment.WebRootPath, trimmedPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}