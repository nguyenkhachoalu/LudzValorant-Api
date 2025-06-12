using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Payloads.Request.AuthRequests
{
    public class Request_Register
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public IFormFile? Avatar { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
