using Microsoft.AspNetCore.Identity;

namespace LoginDemo.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; } 
    }
}
