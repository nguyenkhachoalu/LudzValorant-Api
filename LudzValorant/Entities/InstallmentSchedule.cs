namespace LudzValorant.Entities
{
    public class InstallmentSchedule : BaseEntity
    {
        public Guid PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
