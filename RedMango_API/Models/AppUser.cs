using Microsoft.AspNetCore.Identity;

namespace RedMango_API.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
