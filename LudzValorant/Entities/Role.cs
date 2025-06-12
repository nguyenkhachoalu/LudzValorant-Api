namespace LudzValorant.Entities
{
    public class Role : BaseEntity
    {
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
