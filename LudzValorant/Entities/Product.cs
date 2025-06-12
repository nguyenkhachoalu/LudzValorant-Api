namespace LudzValorant.Entities
{
    public class Product : BaseEntity
    {
        public Guid AccountId { get; set; }
        public Account Account { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
    }
}
