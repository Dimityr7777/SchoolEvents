namespace SchoolEvents.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }

        public int TotalNews { get; set; }
        public int PublishedNews { get; set; }

        public int TotalMessages { get; set; }

        public int TotalAlbums { get; set; }
        public int TotalPhotos { get; set; }
    }
}
