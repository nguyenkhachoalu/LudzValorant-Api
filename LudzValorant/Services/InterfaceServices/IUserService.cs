using LudzValorant.Payloads.ResponseModels.DataUser;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface IUserService
    {
        Task<ResponsePagedResult<DataResponseUser>> GetPagedUser(string? username, int pageNumber, int pageSize);
        Task<ResponseObject<DataResponseUser>> GetUserById(Guid id);
        Task<ResponseObject<string>> UpdateRoleOfUser(Guid id, List<string> roleCode);
        Task<ResponseObject<IEnumerable<string>>> GetRoleOfUser(Guid idUser);
    }
}
