using LudzValorant.Enums;

namespace LudzValorant.Payloads.ResponseModels.DataPurchase
{
    public class DataResponsePagedPurchase : DataResponseBase
    {
        public Guid ProductId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PurchaseStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public string ContactInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
