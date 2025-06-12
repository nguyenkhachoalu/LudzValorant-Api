using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class GunBuddy
    {
        [Key]
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string? DisplayIcon { get; set; }
        public ICollection<AccountGunBuddy> AccountGunBuddies { get; set; }
    }
}
