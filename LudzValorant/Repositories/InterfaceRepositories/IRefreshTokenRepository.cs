using LudzValorant.Entities;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
    }
}
