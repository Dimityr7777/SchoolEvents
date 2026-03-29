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
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NewsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ✅ Публично: списък новини
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.NewsPosts.AsNoTracking()
                .Where(n => n.IsPublished);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(n =>
                    n.Title.Contains(q) || n.Content.Contains(q)
                );
            }

            var list = await query
                .OrderByDescending(n => n.PublishedAt)
                .ToListAsync();

            ViewBag.Q = q;
            return View(list);
        }

        // ✅ Публично: детайли
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _db.NewsPosts.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);

            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ Admin: Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // ✅ Admin: Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(NewsPost model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.PublishedAt = DateTime.Now;
            _db.NewsPosts.Add(model);
            await _db.SaveChangesAsync();

            TempData["NewsSuccess"] = "Новината е добавена успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Admin: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _db.NewsPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ Admin: Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, NewsPost model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var post = await _db.NewsPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Content = model.Content;
            post.CoverImageUrl = model.CoverImageUrl;
            post.IsPublished = model.IsPublished;

            await _db.SaveChangesAsync();

            TempData["NewsSuccess"] = "Новината е обновена успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Admin: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.NewsPosts.FindAsync(id);
            if (post == null) return NotFound();

            _db.NewsPosts.Remove(post);
            await _db.SaveChangesAsync();

            TempData["NewsSuccess"] = "Новината е изтрита.";
            return RedirectToAction(nameof(Index));
        }
    }
}

