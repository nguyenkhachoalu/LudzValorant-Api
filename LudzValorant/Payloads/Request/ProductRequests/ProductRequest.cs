using LudzValorant.Entities;
using LudzValorant.Payloads.Request.AccountRequests;

namespace LudzValorant.Payloads.Request.ProductRequests
{
    public class ProductRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; }
        public Guid AccountId { get; set; }
    }
}
