using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Payloads.Request.AuthRequests
{
    public class Request_ChangePassword
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
