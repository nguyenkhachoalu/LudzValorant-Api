using LudzValorant.Entities;

namespace LudzValorant.Payloads.ResponseModels.DataAccount
{
    public class DataResponseAccountRiotHelper : DataResponseBase
    {
        public Guid UserId { get; set; }
        public string RiotPuuid { get; set; }
        public string PlayerCardImage { get; set; }
        public string GameName { get; set; }
        public string TagLine { get; set; }
        public string Shard { get; set; }
        public string Region { get; set; }
        public DateTime ExpireTime { get; set; }
        public List<DataResponseSkinRiotHelper> Skins { get; set; }

    }
}
