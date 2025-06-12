using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Payloads.Request.AuthRequests
{
    public class Request_Login
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
    }
}
