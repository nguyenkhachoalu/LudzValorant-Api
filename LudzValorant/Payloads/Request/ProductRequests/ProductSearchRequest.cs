using LudzValorant.Enums;

namespace LudzValorant.Payloads.Request.ProductRequests
{
    public class ProductFilterRequest
    {
        public string? Keyword { get; set; }
        public ProductSearchType? ProductSearchType { get; set; }
        public bool? IsPublic { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
