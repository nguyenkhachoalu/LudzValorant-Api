namespace LudzValorant.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<ConfirmEmail> ConfirmEmails { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
