using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class Weapon

    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        public ICollection<Skin> Skins { get; set; }
    }
}
