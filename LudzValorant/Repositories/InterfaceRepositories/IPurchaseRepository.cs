using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Pageds;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IPurchaseRepository
    {
        Task<IPagedList<Purchase>> GetPagedPurchasesByUserIdAsync(
        Guid userId,
        PurchaseStatus? status,
        PaymentMethod? paymentMethod,
        int pageNumber,
        int pageSize);
    }
}
