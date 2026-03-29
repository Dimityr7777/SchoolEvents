using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace SchoolEvents.Models
{
    public class Schoolevent
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = "";

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }

        [StringLength(160)]
        public string? Location { get; set; }

        [StringLength(60)]
        public string? Type { get; set; }

        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
