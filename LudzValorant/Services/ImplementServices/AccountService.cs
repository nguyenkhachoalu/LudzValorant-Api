using LudzValorant.Entities;
using LudzValorant.Helpers;
using LudzValorant.Payloads.Request.AccountRequests;
using LudzValorant.Payloads.ResponseModels.DataAccount;
using LudzValorant.Payloads.ResponseModels.DataUser;
using LudzValorant.Payloads.Responses;
using LudzValorant.Repositories.ImplementRepositories;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.InterfaceServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace LudzValorant.Services.ImplementServices
{
    public class AccountService : IAccountService
    {
        private readonly IBaseRepository<Account> _baseAccountRepository;

        private readonly IBaseRepository<AccountSkin> _baseAccountSkinRepository;
        private readonly IBaseRepository<AccountGunBuddy> _baseAccountGunBuddyRepository;
        private readonly IBaseRepository<AccountAgent> _baseAccountAgentRepository;
        private readonly IBaseRepository<AccountContract> _baseAccountContractRepository;
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IBaseRepository<Product> _baseProductRepository;
        private readonly IAccountRepository _accountRepository;

        public AccountService(IBaseRepository<Account> baseAccountRepository, IBaseRepository<Skin> baseSkinRepository, IBaseRepository<GunBuddy> baseGunBuddyRepository, IBaseRepository<Agent> baseAgentRepository, IBaseRepository<Contract> baseContractRepository, IBaseRepository<AccountSkin> baseAccountSkinRepository, IBaseRepository<AccountGunBuddy> baseAccountGunBuddyRepository, IBaseRepository<AccountAgent> baseAccountAgentRepository, IBaseRepository<AccountContract> baseAccountContractRepository, IBaseRepository<User> baseUserRepository, IBaseRepository<Product> baseProductRepository, IUserRepository userRepository, IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor)
        {
            _baseAccountRepository = baseAccountRepository;

            _baseAccountSkinRepository = baseAccountSkinRepository;
            _baseAccountGunBuddyRepository = baseAccountGunBuddyRepository;
            _baseAccountAgentRepository = baseAccountAgentRepository;
            _baseAccountContractRepository = baseAccountContractRepository;
            _baseUserRepository = baseUserRepository;
            _baseProductRepository = baseProductRepository;

            _accountRepository = accountRepository;
        }

        public async Task<ResponseObject<Guid>> AddAccount(Guid userId, int expireTime, DataResponseGetApiAccount ApiData)
        {
            try
            {
                if(ApiData == null)
                {
                    return new ResponseObject<Guid>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Không lấy được dữ liệu",
                        Data = Guid.NewGuid()
                    };
                }
                var createOwnedItems = await _accountRepository.CreateFullAccountAsync(userId, expireTime, ApiData);

                return new ResponseObject<Guid>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Tài khoản đã được tạo thành công",
                    Data = createOwnedItems
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<Guid>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi thêm tài khoản: {ex.InnerException?.Message ?? ex.Message}",
                    Data = Guid.NewGuid()
                };
            }
        }



        public async Task<ResponseObject<IEnumerable<DataResponseAccountByUserId>>> GetAccountByUserId(Guid userId)
        {
            var user = await _baseUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseObject<IEnumerable<DataResponseAccountByUserId>>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy người dùng",
                    Data = null
                };
            }

            var account = await _baseAccountRepository.GetAllAsync(x => x.UserId == userId);
            if (account == null)
            {
                return new ResponseObject<IEnumerable<DataResponseAccountByUserId>>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy tài khoản",
                    Data = null
                };
            }
            return new ResponseObject<IEnumerable<DataResponseAccountByUserId>>
            {
                Status = StatusCodes.Status200OK,
                Message = "Tài khoản đã được tìm thấy",
                Data = account.Select(s => new DataResponseAccountByUserId
                {
                    Id = s.Id,
                    PlayerCardImage = s.PlayerCardImage,
                    GameName = s.GameName,
                    TagLine = s.TagLine,
                }).ToList()
            };
        }


        public async Task CleanupExpiredAccounts()
        {
            var expiredAccounts = await _baseAccountRepository.GetAllAsync(x => x.ExpireTime < DateTime.UtcNow);
            var expiredList = await expiredAccounts.ToListAsync();

            var deletableAccounts = new List<Account>();

            foreach (var acc in expiredList)
            {
                var product = await _baseProductRepository.GetAsync(x => x.AccountId == acc.Id);
                if (product == null)
                {
                    deletableAccounts.Add(acc);
                }
            }

            if (deletableAccounts.Any())
            {
                var deletableAccountIds = deletableAccounts.Select(a => a.Id).ToList();

                // Xóa account skin trước
                var accountskins = await _baseAccountSkinRepository.GetAllAsync(s => deletableAccountIds.Contains(s.AccountId));
                var accountGunBuddies = await _baseAccountGunBuddyRepository.GetAllAsync(s => deletableAccountIds.Contains(s.AccountId));
                var accountAgent = await _baseAccountAgentRepository.GetAllAsync(s => deletableAccountIds.Contains(s.AccountId));
                var accountContract = await _baseAccountContractRepository.GetAllAsync(s => deletableAccountIds.Contains(s.AccountId));

                var accountskinList = await accountskins.ToListAsync();
                var accountGunbuddyList = await accountGunBuddies.ToListAsync();
                var accountAgentList = await accountAgent.ToListAsync();
                var accountContractList = await accountContract.ToListAsync();



                if (accountskinList.Any())
                {
                    await _baseAccountSkinRepository.DeleteRangeAsync(accountskinList);
                }
                if (accountGunbuddyList.Any())
                {
                    await _baseAccountGunBuddyRepository.DeleteRangeAsync(accountGunbuddyList);
                }
                if (accountAgentList.Any())
                {
                    await _baseAccountAgentRepository.DeleteRangeAsync(accountAgentList);
                }
                if (accountContractList.Any())
                {
                    await _baseAccountContractRepository.DeleteRangeAsync(accountContractList);
                }

                // Rồi mới xóa account
                await _baseAccountRepository.DeleteRangeAsync(deletableAccounts);
            }
        }


        public async Task<ResponseObject<string>> DeleteAccountById(Guid accountId)
        {
            try
            {
                var account = await _baseAccountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Không tìm thấy tài khoản",
                        Data = null
                    };
                }

                await _baseAccountSkinRepository.DeleteAsync(x => x.AccountId == accountId);
                await _baseAccountGunBuddyRepository.DeleteAsync(x => x.AccountId == accountId);
                await _baseAccountAgentRepository.DeleteAsync(x => x.AccountId == accountId);
                await _baseAccountContractRepository.DeleteAsync(x => x.AccountId == accountId);
                await _baseAccountRepository.DeleteAsync(accountId);

                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Xóa tài khoản thành công",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi khi xóa tài khoản: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseObject<DataResponseAccount>> GetAccountById(Guid id, bool? filterTier = false)
        {
            var account = await _accountRepository.GetFullAccountById(id, filterTier.GetValueOrDefault());

            if (account == null)
            {
                return new ResponseObject<DataResponseAccount>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy tài khoản",
                    Data = null
                };
            }

            return new ResponseObject<DataResponseAccount>
            {
                Status = StatusCodes.Status200OK,
                Message = "Tài khoản đã được tìm thấy",
                Data = account
            };
        }


    }
}
