using LudzValorant.DataContexts;
using LudzValorant.Entities;
using LudzValorant.Pageds;
using LudzValorant.Repositories.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace LudzValorant.Repositories.ImplementRepositories
{
    public class UserRepository : IUserRepository
    {
        ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IPagedList<User>> GetPagedUsersByUsernameAsync(string? username, int pageNumber, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                query = query.Where(u => u.Username.Contains(username));
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            return await PagedList<User>.CreateAsync(query, pageNumber, pageSize);
        }
        public async Task AddRolesToUserAsync(User user, List<string> listRoles)
        {

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (listRoles == null)
            {
                throw new ArgumentNullException(nameof(listRoles));
            }
            foreach (var role in listRoles.Distinct())
            {
                var roleOfUser = await GetRolesOfUserAsync(user);
                if (await IsStringInListAsync(role, roleOfUser.ToList()))
                {
                    throw new ArgumentException("Nguời dùng có quyền này rồi");
                }
                else
                {
                    var roleItem = await _context.Roles.SingleOrDefaultAsync(x => x.RoleCode.Equals(role));
                    if (roleItem == null)
                    {
                        throw new ArgumentNullException("Không tồn tại quyền này");
                    }
                    _context.UserRoles.Add(new UserRole
                    {
                        RoleId = roleItem.Id,
                        UserId = user.Id,

                    });

                }
            }
            _context.SaveChanges();
        }

        public async Task<IEnumerable<string>> GetRolesOfUserAsync(User user)
        {
            var roles = new List<string>();
            var listRoles = _context.UserRoles.Where(x => x.UserId == user.Id).AsQueryable();
            foreach (var item in listRoles.Distinct())
            {
                var role = _context.Roles.SingleOrDefault(x => x.Id == item.RoleId);
                roles.Add(role.RoleCode);
            }
            return roles.AsEnumerable();
        }


        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            return user;
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(userName.ToLower()));
            return user;
        }

        #region String processing
        private Task<bool> CompareStringAsync(string str1, string str2)
        {
            return Task.FromResult(string.Equals(str1.ToLowerInvariant(), str2.ToLowerInvariant()));
        }
        private async Task<bool> IsStringInListAsync(string inputString, List<string> listString)
        {
            if (inputString == null)
            {
                throw new ArgumentNullException(nameof(inputString));
            }
            if (listString == null)
            {
                throw new AbandonedMutexException(nameof(listString));
            }
            foreach (var item in listString)
            {
                if (await CompareStringAsync(inputString, item))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

    }
}
