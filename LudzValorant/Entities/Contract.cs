using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class Contract
    {
        [Key]
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public ICollection<AccountContract> AccountContracts { get; set; }
    }
}
