using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolEvents.Models
{
    public class NewsPost
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; } = "";

        [Required, StringLength(8000)]
        public string Content { get; set; } = "";

        [StringLength(300)]
        public string? CoverImageUrl { get; set; } // по избор: линк към снимка

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}
