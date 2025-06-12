namespace LudzValorant.Entities
{
    public class AccountGunBuddy
    {
        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        public string GunBuddyId { get; set; }
        public GunBuddy GunBuddy { get; set; }
    }
}
