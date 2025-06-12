using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Payloads.Request.PurchaseRequests;
using LudzValorant.Payloads.ResponseModels.DataPurchase;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface IPurchaseService
    {
        Task<ResponseObject<string>> Purchase(PurchaseRequest request);
        Task<ResponseObject<string>> UpdatePurchase(Guid purchaseId, PurchaseUpdateRequest request);
        Task<ResponsePagedResult<DataResponsePagedPurchase>> GetPagedPurchaseByUserId(Guid userId, PurchaseStatus? status, PaymentMethod? paymentMethod, int pageNumber,int pageSize);
        Task<ResponseObject<DataResponsePurchase>> GetPurchaseById(Guid id);

    }
}
