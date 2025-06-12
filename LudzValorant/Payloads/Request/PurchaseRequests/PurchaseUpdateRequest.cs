using LudzValorant.Enums;

namespace LudzValorant.Payloads.Request.PurchaseRequests
{
    public class PurchaseUpdateRequest
    {
        public int TotalAmount { get; set; }
        public string? Note { get; set; }
        public string ContactInfo { get; set; }
    }
}
