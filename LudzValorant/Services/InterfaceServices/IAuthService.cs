
using LudzValorant.Entities;
using LudzValorant.Payloads.Request.AuthRequests;
using LudzValorant.Payloads.ResponseModels.DataAuth;
using LudzValorant.Payloads.ResponseModels.DataUser;
using LudzValorant.Payloads.Responses;

namespace LudzValorant.Services.InterfaceServices
{
    public interface IAuthService
    {
        Task<ResponseObject<DataResponseUser>> Register(Request_Register request);
        Task<string> ConfirmRegisterAccount(string confirmCode);
        Task<ResponseObject<DataResponseLogin>> Login(Request_Login request);
        Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user);

        Task<ResponseObject<DataResponseUser>> ChangePassword(Guid userId, Request_ChangePassword request);
        Task<string> ForgotPassword(string userName);
        Task<string> ConfirmForgotPassword(string userName, string confirmCode);
        Task<ResponseObject<string>> LogoutAsync(string token);
        Task<ResponseObject<DataResponseLogin>> RefreshTokenAsync(string refreshToken);
    }
}
