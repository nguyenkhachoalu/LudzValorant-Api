using LudzValorant.Payloads.ResponseModels.DataAccount;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IAccountRepository
    {
        Task<DataResponseAccount?> GetFullAccountById(Guid accountId, bool filterTier = false);
        Task<Guid> CreateFullAccountAsync(Guid userId,int expireTime, DataResponseGetApiAccount data);
    }
}
