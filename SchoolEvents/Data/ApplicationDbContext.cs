using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolEvents.Models;

namespace SchoolEvents.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Schoolevent> Schoolevents { get; set; } = default!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = default!;
        public DbSet<GalleryPhoto> GalleryPhotos { get; set; } = default!;
        public DbSet<NewsPost> NewsPosts { get; set; } = default!;
        public DbSet<GalleryAlbum> GalleryAlbums { get; set; } = default!;
    }
}