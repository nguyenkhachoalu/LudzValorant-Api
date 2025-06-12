using LudzValorant.Payloads.Request.AccountRequests;

namespace LudzValorant.Payloads.Request.ProductRequests
{
    public class UpdateProductRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsPublic { get; set; }
    }
}
