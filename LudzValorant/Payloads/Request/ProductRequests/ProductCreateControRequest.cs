using LudzValorant.Payloads.Request.AccountRequests;

namespace LudzValorant.Payloads.Request.ProductRequests
{
    public class ProductCreateControRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? image { get; set; }
        public AccountRequest AccountRequest { get; set; }
    }
}
