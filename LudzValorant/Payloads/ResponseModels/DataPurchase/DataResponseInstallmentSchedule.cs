using LudzValorant.Entities;

namespace LudzValorant.Payloads.ResponseModels.DataPurchase
{
    public class DataResponseInstallmentSchedule : DataResponseBase
    {
        public Guid PurchaseId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
