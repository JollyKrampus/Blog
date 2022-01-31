using Microsoft.AspNetCore.Identity;

namespace BlazorBlogsLibrary.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public bool NewsletterSubscriber { get; set; }
    }
}