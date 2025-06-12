
namespace LudzValorant.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsActive { get; set; }
        public User? User { get; set; }
    }
}
