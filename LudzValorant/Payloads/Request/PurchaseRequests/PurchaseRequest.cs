using LudzValorant.Entities;
using LudzValorant.Enums;

namespace LudzValorant.Payloads.Request.PurchaseRequests
{
    public class PurchaseRequest
    {
        public Guid ProductId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountOfFirstPayment { get; set; }
        public string? Note { get; set; }
        public int InstallmentPeriod { get; set; }
        public string ContactInfo { get; set; }
    }
}
