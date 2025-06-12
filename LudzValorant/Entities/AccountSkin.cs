namespace LudzValorant.Entities
{
    public class AccountSkin
    {
        public Guid AccountId { get; set; }
        public string SkinId { get; set; }
        public Account Account { get; set; }
        public Skin Skin { get; set; }

    }
}
