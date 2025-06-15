namespace LudzValorant.Entities
{
    public class Account : BaseEntity
    {
        public Guid UserId { get; set; }
        public string RiotPuuid { get; set; }
        public string Shard { get; set; }
        public int Level { get; set; }
        public string RankName { get; set; }
        public string Region { get; set; }
        public string GameName { get; set; }
        public string TagLine { get; set; }
        public string PlayerCardImage { get; set; }
        public DateTime ExpireTime { get; set; }

        public ICollection<AccountSkin> AccountSkins { get; set; }
        public ICollection<AccountContract> AccountContracts { get; set; }
        public ICollection<AccountGunBuddy> AccountGunBuddies { get; set; }
        public ICollection<AccountAgent> AccountAgents { get; set; }
    }

}
