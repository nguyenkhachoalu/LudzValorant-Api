
namespace LudzValorant.Payloads.Request.PurchaseRequests
{
    public class InstallmentScheduleRequest
    {
        public Guid PurchaseId { get; set; }
        public decimal Amount { get; set; }
    }
}
