using LudzValorant.Enums;
using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class Skin 
    {

        [Key]
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string? DisplayIcon { get; set; }
        public decimal? Price { get; set; }
        public string TierId { get; set; }
        public string WeaponId { get; set; }

        public Tier Tier { get; set; }
        public Weapon Weapon { get; set; }
        public ICollection<AccountSkin> AccountSkins { get; set; }
    }
}
