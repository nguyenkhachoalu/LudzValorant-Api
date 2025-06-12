using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class Tier
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string? TierImage { get; set; }


        public ICollection<Skin> Skins { get; set; }
    }
}
