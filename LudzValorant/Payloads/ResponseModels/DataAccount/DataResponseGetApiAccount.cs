using LudzValorant.Entities;

namespace LudzValorant.Payloads.ResponseModels.DataAccount
{
    public class DataResponseGetApiAccount
    {
        public string RiotPuuid { get; set; }
        public string PlayerCardImage { get; set; }
        public int Level { get; set; }
        public string Rank { get; set; }
        public string GameName { get; set; }
        public string TagLine { get; set; }
        public string Shard { get; set; }
        public string Region { get; set; }
        public DateTime ExpireTime { get; set; }
        public List<string> SkinIds { get; set; }
        public List<string> AgentIds { get; set; }
        public List<string> ContractIds { get; set; }
        public List<string> GunBuddyIds { get; set; }
    }
}
