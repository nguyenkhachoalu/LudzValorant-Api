using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Pageds;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IProductRepository
    {
        Task<IPagedList<Product>> GetPagedProductsBySkinNameAsync(
     Guid? ownerId,
     string? keyword,
     ProductSearchType? productSearchType,
     bool? isPublic,
     decimal? minPrice,
     decimal? maxPrice,
     int pageNumber,
     int pageSize);

    }
}
