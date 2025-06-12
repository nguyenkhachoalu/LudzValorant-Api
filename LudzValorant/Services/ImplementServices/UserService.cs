using LudzValorant.Entities;
using LudzValorant.Payloads.Mappers;
using LudzValorant.Payloads.ResponseModels.DataUser;
using LudzValorant.Payloads.Responses;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.InterfaceServices;

namespace LudzValorant.Services.ImplementServices
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IBaseRepository<UserRole> _baseUserRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IBaseRepository<User> baseUserRepository, IBaseRepository<UserRole> baseUserRoleRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _baseUserRepository = baseUserRepository;
            _baseUserRoleRepository = baseUserRoleRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponsePagedResult<DataResponseUser>> GetPagedUser(string? username, int pageNumber, int pageSize)
        {
            try
            {
                var users = await _userRepository.GetPagedUsersByUsernameAsync(username, pageNumber, pageSize);
                var hostUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                var responseUsers = users.Items.Select(t => new DataResponseUser
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Username = t.Username,
                    Avatar = $"{hostUrl}/images/avatars/{t.Avatar}",
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                }).ToList();

                return new ResponsePagedResult<DataResponseUser>
                {
                    Items = responseUsers,
                    TotalPages = users.TotalPages,
                    TotalItems = users.TotalItems,
                    PageNumber = users.PageNumber,
                    PageSize = users.PageSize
                };
            }
            catch (Exception ex)
            {
                return new ResponsePagedResult<DataResponseUser>
                {
                    Items = new List<DataResponseUser>(),
                    TotalPages = 0,
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public async Task<ResponseObject<DataResponseUser>> GetUserById(Guid id)
        {
            var hostUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            var user = await _baseUserRepository.GetAsync(x => x.Id == id);
            if (user == null)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Message = "Không tìm thấy người dùng",
                    Data = null,
                };

            }
            var response = new DataResponseUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                Avatar = $"{hostUrl}/images/avatars/{user.Avatar}",
                CreatedAt = user.CreatedAt,
            };
            return new ResponseObject<DataResponseUser>
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy thông tin người dùng thành công",
                Data = response,
            };
        }
        public async Task<ResponseObject<IEnumerable<string>>> GetRoleOfUser(Guid idUser)
        {
            var user = await _baseUserRepository.GetAsync(x => x.Id == idUser);
            if (user == null)
            {
                return new ResponseObject<IEnumerable<string>>
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Message = "người dùng không tồn tại",
                    Data = null,
                };
            }
            var rolesOfUser = await _userRepository.GetRolesOfUserAsync(user);
            return new ResponseObject<IEnumerable<string>>
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy danh sách thành công",
                Data = rolesOfUser,
            };
        }
        public async Task<ResponseObject<string>> UpdateRoleOfUser(Guid id, List<string> roleCode)
        {
            try
            {
                var user = await _baseUserRepository.GetAsync(x => x.Id == id);
                if (user == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Người dùng không tồn tại",
                        Data = null,
                    };
                }
                var roles = await _userRepository.GetRolesOfUserAsync(user);
                bool isAdmin = roles.Any(r => r == "Admin");
                if (isAdmin && !roleCode.Contains("Admin"))
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Bạn không có quyền xóa quyền Admin.",
                        Data = null,
                    };
                }

                // Lấy toàn bộ UserRole hiện tại của user
                var currentUserRoles = await _baseUserRoleRepository.GetAllAsync(ur => ur.UserId == id);
                if (currentUserRoles.Any())
                {
                   await _baseUserRoleRepository.DeleteRangeAsync(currentUserRoles);
                }

                // Thêm quyền mới
                await _userRepository.AddRolesToUserAsync(user, roleCode);
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Thêm quyền thành công",
                    Data = null,
                };
            }

            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null,
                };

            }
        }
    }
}
