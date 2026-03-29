using System;
using System.Collections.Generic;

namespace SchoolEvents.Models
{
    public class GalleryAlbum
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Slug { get; set; } = "";          // за красиви URL-и (по избор)
        public string? CoverPath { get; set; }          // миниатюра/cover
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<GalleryPhoto> Photos { get; set; } = new();
    }
}
