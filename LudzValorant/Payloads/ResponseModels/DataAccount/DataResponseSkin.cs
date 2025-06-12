using LudzValorant.Enums;

namespace LudzValorant.Payloads.ResponseModels.DataAccount
{
    public class DataResponseSkin
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string DisplayIcon { get; set; }
        public decimal? Price { get; set; }
        public string TierId { get; set; }
        public string TierImage { get; set; }
    }
}
