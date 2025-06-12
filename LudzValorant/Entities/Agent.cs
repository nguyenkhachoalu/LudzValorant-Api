using System.ComponentModel.DataAnnotations;

namespace LudzValorant.Entities
{
    public class Agent
    {
        [Key]
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string? FullPortrait { get; set; }
        public ICollection<AccountAgent> AccountAgents { get; set; }
    }
}
