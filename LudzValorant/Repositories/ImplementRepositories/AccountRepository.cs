using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Payloads.ResponseModels.DataAccount;
using LudzValorant.Repositories.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace LudzValorant.Repositories.ImplementRepositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataResponseAccount?> GetFullAccountById(Guid accountId, bool filterTier = false)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
                return null;

            var skins = await _context.AccountSkins
                .Where(x => x.AccountId == accountId)
                .Include(x => x.Skin)
                    .ThenInclude(s => s.Tier)
                .Include(x => x.Skin.Weapon)
                .Select(x => x.Skin)
                .ToListAsync();

            var tierPriorityOrder = new[]
            {
                "411e4a55-4e59-7757-41f0-86a53f101bb5", // Ultra
                "e046854e-406c-37f4-6607-19a9ba8426fc", // Exclusive
                "60bca009-4182-7998-dee7-b8a2558dc369", // Premium
                "12683d76-48d7-84a3-4e09-6985794f0445", // Select
                "0cebb8be-46d7-c12a-d306-e9907bfc5a25"  // Deluxe
            };

            if (filterTier)
            {
                skins = skins.Where(s => tierPriorityOrder.Take(3).Contains(s.TierId)).ToList();
            }

            var preferredOrder = new[] { "Vandal", "Phantom", "Melee", "Operator", "Sheriff", "Classic" };

            var groupedWeapons = skins
                .GroupBy(s => new { s.WeaponId, s.Weapon.Name })
                .Select(g => new DataResponseWeapon
                {
                    Id = g.Key.WeaponId,
                    Name = g.Key.Name,
                    Skins = g.OrderBy(s =>
                    {
                        var index = Array.IndexOf(tierPriorityOrder, s.TierId);
                        return index == -1 ? int.MaxValue : index;
                    })
                    .Select(s => new DataResponseSkin
                    {
                        Id = s.Id,
                        DisplayName = s.DisplayName,
                        DisplayIcon = s.DisplayIcon,
                        Price = s.Price,
                        TierId = s.TierId,
                        TierImage = s.Tier.TierImage
                    }).ToList()
                })
                .OrderBy(w => preferredOrder.Contains(w.Name) ? Array.IndexOf(preferredOrder, w.Name) : int.MaxValue)
                .ToList();

            var agents = await _context.AccountAgents
                .Where(x => x.AccountId == accountId)
                .Include(x => x.Agent)
                .Select(x => x.Agent)
                .ToListAsync();

            var gunbuddies = await _context.AccountGunBuddies
                .Where(x => x.AccountId == accountId)
                .Include(x => x.GunBuddy)
                .Select(x => x.GunBuddy)
                .ToListAsync();

            var contracts = await _context.AccountContracts
                .Where(x => x.AccountId == accountId)
                .Include(x => x.Contract)
                .Select(x => x.Contract)
                .ToListAsync();

            return new DataResponseAccount
            {
                Id = account.Id,
                UserId = account.UserId,
                RiotPuuid = account.RiotPuuid,
                PlayerCardImage = account.PlayerCardImage,
                Level = account.Level,
                RankName = account.RankName,
                GameName = account.GameName,
                TagLine = account.TagLine,
                Shard = account.Shard,
                Region = account.Region,
                ExpireTime = account.ExpireTime,
                Weapons = groupedWeapons,
                Agents = agents.Select(a => new DataResponseAgent
                {
                    Id = a.Id,
                    DisplayName = a.DisplayName,
                    FullPortrait = a.FullPortrait
                }).ToList(),
                Buddies = gunbuddies.Select(b => new DataResponseGunBuddy
                {
                    Id = b.Id,
                    DisplayName = b.DisplayName,
                    DisplayIcon = b.DisplayIcon,
                }).ToList(),
                Contracts = contracts.Select(c => new DataResponseContract
                {
                    Id = c.Id,
                    DisplayName = c.DisplayName
                }).ToList()
            };
        }


        public async Task<Guid> CreateFullAccountAsync(Guid userId, int expireTime, DataResponseGetApiAccount data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.Role.RoleCode)
                    .ToListAsync();
                if (!roles.Any(r => r == "ADMIN" || r == "MOD"))
                {
                    var oldAccounts = await _context.Accounts
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                    foreach (var acc in oldAccounts)
                    {
                        _context.AccountSkins.RemoveRange(_context.AccountSkins.Where(x => x.AccountId == acc.Id));
                        _context.AccountAgents.RemoveRange(_context.AccountAgents.Where(x => x.AccountId == acc.Id));
                        _context.AccountGunBuddies.RemoveRange(_context.AccountGunBuddies.Where(x => x.AccountId == acc.Id));
                        _context.AccountContracts.RemoveRange(_context.AccountContracts.Where(x => x.AccountId == acc.Id));
                    }

                    _context.Accounts.RemoveRange(oldAccounts);
                    await _context.SaveChangesAsync();
                }
                var account = new Account
                {
                    UserId = userId,
                    RiotPuuid = data.RiotPuuid,
                    PlayerCardImage = data.PlayerCardImage,
                    Level = data.Level,
                    RankName = data.Rank,
                    GameName = data.GameName,
                    TagLine = data.TagLine,
                    Shard = data.Shard,
                    Region = data.Region,
                    ExpireTime = DateTime.UtcNow.AddDays(expireTime),
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                _context.AccountSkins.AddRange(data.SkinIds.Select(id => new AccountSkin { AccountId = account.Id, SkinId = id }));
                _context.AccountAgents.AddRange(data.AgentIds.Select(id => new AccountAgent { AccountId = account.Id, AgentId = id }));
                _context.AccountGunBuddies.AddRange(data.GunBuddyIds.Select(id => new AccountGunBuddy { AccountId = account.Id, GunBuddyId = id }));
                _context.AccountContracts.AddRange(data.ContractIds.Select(id => new AccountContract { AccountId = account.Id, ContractId = id }));

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return account.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }


}
