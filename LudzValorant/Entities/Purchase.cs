using LudzValorant.Enums;

namespace LudzValorant.Entities
{
    public class Purchase : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PurchaseStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; }
        public string ContactInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<InstallmentSchedule> Installments { get; set; }
    }
}
