using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Repositories.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;

namespace LudzValorant.Repositories.ImplementRepositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }
    }
}
