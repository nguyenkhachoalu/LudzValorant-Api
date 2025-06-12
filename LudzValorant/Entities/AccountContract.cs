namespace LudzValorant.Entities
{
    public class AccountContract
    {
        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        public string ContractId { get; set; }
        public Contract Contract { get; set; }
    }
}
