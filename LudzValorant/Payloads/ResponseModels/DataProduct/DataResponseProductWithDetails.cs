using LudzValorant.Payloads.ResponseModels.DataAccount;

namespace LudzValorant.Payloads.ResponseModels.DataProduct
{
    public class DataResponseProductWithDetails
    {
        public Guid Id { get; set; } // từ BaseEntity
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }

        public string Image { get; set; } // danh sách ảnh của product

        public DataResponseAccount Account { get; set; }
    }
}
