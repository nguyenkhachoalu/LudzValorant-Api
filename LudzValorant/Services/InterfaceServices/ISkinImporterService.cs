using LudzValorant.Payloads.ResponseModels.DataAccount;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface ISkinImporterService
    {
        Task ImportAllSkinsAsync();
        Task<ResponseObject<DataResponseGetApiAccount>> GetAccountByUrlRiot(string url);
    }
}
