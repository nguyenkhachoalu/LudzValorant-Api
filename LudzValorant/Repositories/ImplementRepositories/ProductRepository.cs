using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Pageds;
using LudzValorant.Repositories.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;

namespace LudzValorant.Repositories.ImplementRepositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IPagedList<Product>> GetPagedProductsBySkinNameAsync(
         Guid? ownerId,
         string? keyword,
         ProductSearchType? productSearchType,
         bool? isPublic,
         int pageNumber,
         int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Owner)
                .Include(p => p.Account)
                    .ThenInclude(a => a.AccountSkins)
                        .ThenInclude(asn => asn.Skin)
                .AsQueryable();

            if (ownerId.HasValue)
            {
                query = query.Where(p => p.OwnerId == ownerId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                if (productSearchType == null || productSearchType == ProductSearchType.TITLE)
                {
                    query = query.Where(p => p.Title.Contains(keyword));
                }
                else if (productSearchType == ProductSearchType.SKIN)
                {
                    query = query.Where(p =>
                        p.Account.AccountSkins.Any(asn =>
                            asn.Skin.DisplayName.Contains(keyword)));
                }
            }

            if (isPublic.HasValue)
            {
                query = query.Where(p => p.IsPublic == isPublic.Value);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            return await PagedList<Product>.CreateAsync(query, pageNumber, pageSize);
        }






    }
}
