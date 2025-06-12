

namespace LudzValorant.Entities
{
    public class ConfirmEmail : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ConfirmCode { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public bool IsConfirm { get; set; }
        public User? User { get; set; }
    }
}
