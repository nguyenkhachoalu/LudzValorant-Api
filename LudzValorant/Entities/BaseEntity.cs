using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
