using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Data;
using SchoolEvents.Models;

namespace SchoolEvents.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public GalleryController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ===================== ПУБЛИЧНИ =====================

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var albums = await _db.GalleryAlbums
                .Include(a => a.Photos)
                .OrderBy(a => a.Id) // старите първи, новите последни
                .ToListAsync();

            return View(albums);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Album(int id, string sort = "new")
        {
            var album = await _db.GalleryAlbums
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null) return NotFound();

            var sorted = sort switch
            {
                "name" => album.Photos.OrderBy(p => p.Title).ToList(),
                "old" => album.Photos.OrderBy(p => p.CreatedAt).ToList(),
                _ => album.Photos.OrderByDescending(p => p.CreatedAt).ToList(),
            };

            album.Photos = sorted;
            ViewBag.Sort = sort;

            return View(album);
        }

        // ===================== CREATE ALBUM =====================

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAlbum()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAlbum(string title, string? description)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("", "Името е задължително.");
                return View();
            }

            title = title.Trim();
            description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            var exists = await _db.GalleryAlbums.AnyAsync(a => a.Title == title);
            if (exists)
            {
                ModelState.AddModelError("", "Вече има категория с това име.");
                return View();
            }

            var album = new GalleryAlbum
            {
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _db.GalleryAlbums.Add(album);
            await _db.SaveChangesAsync();

            TempData["GallerySuccess"] = "Категорията е създадена успешно.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== UPLOAD =====================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upload()
        {
            ViewBag.Albums = await _db.GalleryAlbums
                .OrderBy(a => a.Id)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int albumId, string title, string? description, IFormFile file)
        {
            if (albumId <= 0)
                ModelState.AddModelError("", "Избери албум.");

            if (string.IsNullOrWhiteSpace(title))
                ModelState.AddModelError("", "Заглавието е задължително.");

            if (file == null || file.Length == 0)
                ModelState.AddModelError("", "Избери снимка.");

            if (!ModelState.IsValid)
            {
                ViewBag.Albums = await _db.GalleryAlbums.OrderBy(a => a.Id).ToListAsync();
                return View();
            }

            var albumExists = await _db.GalleryAlbums.AnyAsync(a => a.Id == albumId);
            if (!albumExists)
            {
                ModelState.AddModelError("", "Невалиден албум.");
                ViewBag.Albums = await _db.GalleryAlbums.OrderBy(a => a.Id).ToListAsync();
                return View();
            }

            // ✅ валидирай разширение

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("", "Разрешени формати: JPG, PNG, WEBP.");
                ViewBag.Albums = await _db.GalleryAlbums.OrderBy(a => a.Id).ToListAsync();
                return View();
            }

            title = title.Trim();
            description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            // ✅ запис на файл
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ запис в БД
            var photo = new GalleryPhoto
            {
                AlbumId = albumId,
                Title = title,
                Description = description,
                FilePath = $"/uploads/gallery/{fileName}",
                CreatedAt = DateTime.UtcNow
            };

            _db.GalleryPhotos.Add(photo);
            await _db.SaveChangesAsync();

            TempData["GallerySuccess"] = "Снимката е качена успешно.";
            return RedirectToAction(nameof(Album), new { id = albumId });
        }

        // ===================== DELETE PHOTO =====================
        // ✅ ТОВА Е ЕДИНСТВЕНИЯТ DeletePhoto. НЯМА втори със същото име.

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _db.GalleryPhotos.FirstOrDefaultAsync(p => p.Id == id);
            if (photo == null) return NotFound();

            var albumId = photo.AlbumId;

            // изтрий файла
            var physicalPath = Path.Combine(_env.WebRootPath, photo.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _db.GalleryPhotos.Remove(photo);
            await _db.SaveChangesAsync();

            TempData["GallerySuccess"] = "Снимката е изтрита.";
            return RedirectToAction(nameof(Album), new { id = albumId });
        }
    }
}
