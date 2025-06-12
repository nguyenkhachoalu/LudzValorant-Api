using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Pageds;
using LudzValorant.Repositories.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;
namespace LudzValorant.Repositories.ImplementRepositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IPagedList<Purchase>> GetPagedPurchasesByUserIdAsync(
            Guid userId,
            PurchaseStatus? status,
            PaymentMethod? paymentMethod,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Purchases
                .Include(p => p.Installments)
                .Include(p => p.Product)
                .ThenInclude(p => p.Account)
                .Where(p => p.Product.OwnerId == userId) // Sản phẩm thuộc về user
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (paymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            return await PagedList<Purchase>.CreateAsync(query, pageNumber, pageSize);
        }
    }
}
