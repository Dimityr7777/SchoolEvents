using Microsoft.AspNetCore.Identity;

namespace SchoolEvents.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool WantsEmailNotifications { get; set; }
    }
}
