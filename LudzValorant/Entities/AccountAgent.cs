namespace LudzValorant.Entities
{
    public class AccountAgent
    {
        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        public string AgentId { get; set; }
        public Agent Agent { get; set; }
    }
}
