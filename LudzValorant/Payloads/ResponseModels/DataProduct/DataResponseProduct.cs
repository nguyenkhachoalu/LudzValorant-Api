namespace LudzValorant.Payloads.ResponseModels.DataProduct
{
    public class DataResponseProduct : DataResponseBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid AccountId{ get; set; }
        public bool IsPublic { get; set; }

        public string Image { get; set; }
    }
}
