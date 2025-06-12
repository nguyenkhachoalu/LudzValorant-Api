using LudzValorant.Enums;

namespace LudzValorant.Payloads.Request.AccountRequests
{
    public class SkinRequest
    {
        public string SkinId { get; set; }
        public string Name { get; set; }
        public string SkinImage { get; set; }
        public WeaponType WeaponType { get; set; }
    }
}
