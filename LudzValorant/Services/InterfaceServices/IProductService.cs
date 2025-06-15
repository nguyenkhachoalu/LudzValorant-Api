using LudzValorant.Enums;
using LudzValorant.Payloads.Request.ProductRequests;
using LudzValorant.Payloads.ResponseModels.DataProduct;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface IProductService
    {
        Task<ResponseObject<string>> AddProductAsync(Guid userId, ProductRequest request);
        Task<ResponseObject<DataResponseProductWithDetails>> GetProductByIdAsync(Guid productId, bool? filterTier);
        Task<ResponseObject<IEnumerable<DataResponseProduct>>> GetAllProductsByOwnerId(Guid ownerId);
        Task<ResponsePagedResult<DataResponseProduct>> GetPagedProductsByOwnerId(Guid ownerId, ProductFilterRequest request);
        Task<ResponsePagedResult<DataResponseProduct>> GetPagedProducts(ProductFilterRequest request);
        Task<ResponseObject<string>> DeletedProduct(Guid userId, Guid productId);
        Task<ResponseObject<string>> EditProduct(Guid userId, UpdateProductRequest request);

    }
}
