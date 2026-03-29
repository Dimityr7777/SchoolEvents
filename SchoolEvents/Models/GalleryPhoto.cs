using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models
{
    public class GalleryPhoto
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public GalleryAlbum Album { get; set; } = null!;

        [Required, StringLength(140)]
        public string Title { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        // път до файла: /uploads/gallery/xxx.jpg
        [Required, StringLength(300)]
        public string FilePath { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}


