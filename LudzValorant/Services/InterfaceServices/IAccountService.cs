using LudzValorant.Payloads.Request.AccountRequests;
using LudzValorant.Payloads.ResponseModels.DataAccount;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface IAccountService
    {
        Task<ResponseObject<Guid>> AddAccount(Guid userId, int expireTime, DataResponseGetApiAccount ApiData );
        Task<ResponseObject<IEnumerable<DataResponseAccountByUserId>>> GetAccountByUserId(Guid userId);
        Task<ResponseObject<DataResponseAccount>> GetAccountById(Guid id, bool? filterTier = false);
        Task CleanupExpiredAccounts();
        Task<ResponseObject<string>> DeleteAccountById(Guid accountId);

    }
}
