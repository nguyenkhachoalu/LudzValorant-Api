using LudzValorant.Entities;
using LudzValorant.Pageds;

namespace LudzValorant.Repositories.InterfaceRepositories
{
    public interface IUserRepository
    {
        Task AddRolesToUserAsync(User user, List<string> listRoles);
        Task<IEnumerable<string>> GetRolesOfUserAsync(User user);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUserName(string userName);
        Task<IPagedList<User>> GetPagedUsersByUsernameAsync(string? username, int pageNumber, int pageSize);

    }
}
