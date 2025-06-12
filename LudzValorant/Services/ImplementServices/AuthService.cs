using LudzValorant.Validations;
using LudzValorant.Entities;
using LudzValorant.Handle.HandleEmail;
using LudzValorant.Handle.HandleFile;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.InterfaceService;
using LudzValorant.Payloads.Mappers;
using LudzValorant.Payloads.Request.AuthRequests;
using LudzValorant.Payloads.ResponseModels.DataAuth;
using LudzValorant.Payloads.ResponseModels.DataUser;
using LudzValorant.Payloads.Responses;
using LudzValorant.Services.InterfaceServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BcryptNet = BCrypt.Net.BCrypt;
namespace LudzValorant.Services.ImplementServices
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IBaseRepository<UserRole> _baseUserRoleRepository;
        private readonly IBaseRepository<Role> _baseRoleRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IBaseRepository<ConfirmEmail> _baseConfirmEmailRepository;
        private readonly IEmailService _emailService;
        private readonly UserConverter _userConverter;

        public AuthService(IUserRepository userRepository, IBaseRepository<User> baseUserRepository, IBaseRepository<UserRole> baseUserRoleRepository, IBaseRepository<Role> baseRoleRepository, IBaseRepository<RefreshToken> baseRefreshTokenRepository, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IBaseRepository<ConfirmEmail> baseConfirmEmailRepository, IEmailService emailService, UserConverter userConverter)
        {
            _userRepository = userRepository;
            _baseUserRepository = baseUserRepository;
            _baseUserRoleRepository = baseUserRoleRepository;
            _baseRoleRepository = baseRoleRepository;
            _baseRefreshTokenRepository = baseRefreshTokenRepository;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _baseConfirmEmailRepository = baseConfirmEmailRepository;
            _emailService = emailService;
            _userConverter = userConverter;
        }

        public async Task<ResponseObject<DataResponseUser>> ChangePassword(Guid userId, Request_ChangePassword request)
        {
            try
            {
                var user = await _baseUserRepository.GetAsync(x => x.Id == userId);
                bool checkPass = BcryptNet.Verify(request.OldPassword, user.PasswordHash);
                if (!checkPass)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không chính xác",
                        Data = null
                    };
                }
                user.PasswordHash = BcryptNet.HashPassword(request.NewPassword);
                await _baseUserRepository.UpdateAsync(user);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Mật khẩu thay đổi thành công",
                    Data = _userConverter.EntitytoDTO(user)
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null,
                };
            }

        }

        public async Task<string> ConfirmForgotPassword(string userName, string confirmCode)
        {
            try
            {
                if(confirmCode==null || userName == null)
                {
                    return "Không được để trống";
                }
                var user = await _userRepository.GetUserByUserName(userName);
                if (user.IsActive == false)
                {
                    return "Tài khoản đang bị khóa";
                }
                var confirmEmail = await _baseConfirmEmailRepository.GetAsync(x => x.ConfirmCode.Equals(confirmCode));
                if(confirmEmail == null)
                {
                    return "Tài khoản đang bị khóa";
                }
                if (confirmEmail.UserId != user.Id || confirmCode == null)
                {
                    return "Xác thực không thành công";
                }
                if (confirmEmail.IsConfirm == true || confirmEmail.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác thực không còn khả dụng";
                }
                var newPassword = GenerateRandomString();
                user.PasswordHash = BcryptNet.HashPassword(newPassword);
                await _baseUserRepository.UpdateAsync(user);
                var emailContent = _emailService.GenerateForgotPassword(newPassword);
                var message = new EmailMessage(new string[] { user.Email }, $"Mật khẩu mới của {user.Username} ", emailContent);
                var responseMessage = _emailService.SendEmail(message);
                return "Chúng tôi vừa gửi mật khẩu mới về email của bạn, vui lòng kiểm tra email";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ConfirmRegisterAccount(string confirmCode)
        {
            try
            {
                var code = await _baseConfirmEmailRepository.GetAsync(x => x.ConfirmCode.Equals(confirmCode));
                if (code == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                if (code.IsConfirm)
                {
                    return "Mã đã được sử dụng";
                }
                var user = await _baseUserRepository.GetAsync(x => x.Id == code.UserId);
                if (code.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                user.IsActive = true;
                code.IsConfirm = true;
                await _baseUserRepository.UpdateAsync(user);
                await _baseConfirmEmailRepository.UpdateAsync(code);
                return "Xác nhận đăng ký thành công";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> ForgotPassword(string userName)
        {
            try
            {
                if (userName == null)
                {
                    return "Tài khoản tồn tại trong hệ thống";
                }
                var user = await _userRepository.GetUserByUserName(userName);
                if (user == null)
                {
                    return "Tài khoản không tồn tại trong hệ thống";
                }
                
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    UserId = user.Id,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(3),
                    IsConfirm = false,

                };
                await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var emailContent = _emailService.GenerateConfirmationCodeEmail(confirmEmail.ConfirmCode);
                var message = new EmailMessage(new string[] { user.Email }, $"Mã xác nhận của {user.Username} ", emailContent);
                var responseMessage = _emailService.SendEmail(message);

                return "Hệ thống đã gửi mã xác minh tới email của bạn, thời hạn 3 phút";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user)
        {
            var userRoles = await _baseUserRoleRepository.GetAllAsync(x => x.UserId == user.Id);
            var roles = await _baseRoleRepository.GetAllAsync();
            var authClaims = new List<Claim>
            {
                    new Claim("Id",user.Id.ToString()),
                    new Claim("FullName", user.FullName.ToString()),
                    new Claim("Username", user.Username.ToString()),
                    new Claim("Email", user.Email.ToString()),
            };
            foreach (var userRole1 in userRoles)
            {
                foreach (var role in roles)
                {
                    if (role.Id == userRole1.RoleId)
                    {
                        authClaims.Add(new Claim("Role", role.RoleCode));

                    }
                }
            }
            var userRole = await _userRepository.GetRolesOfUserAsync(user);
            foreach (var item in userRole)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }
            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidity"], out int refreshTokenValidity);

            RefreshToken rf = new RefreshToken
            {
                ExpiryTime = DateTime.Now.AddHours(refreshTokenValidity),
                UserId = user.Id,
                Token = refreshToken,
                CreateTime = DateTime.Now,
                IsActive = true,
            };
            rf = await _baseRefreshTokenRepository.CreateAsync(rf);
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status201Created,
                Message = "Tạo token thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    RefreshToken = refreshToken,
                }
            };
        }

        public async Task<ResponseObject<DataResponseLogin>> Login(Request_Login request)
        {
            try
            {
                var user = await _baseUserRepository.GetAsync(x => x.Username.Equals(request.Username));
                
                if (user == null )
                {
                    return new ResponseObject<DataResponseLogin>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Tài khoản hoặc mật khẩu không chính xác",
                        Data = null,
                    };
                }
                bool checkPass = BcryptNet.Verify(request.PasswordHash, user.PasswordHash);
                if (!checkPass)
                {
                    return new ResponseObject<DataResponseLogin>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Tài khoản hoặc mật khẩu không chính xác",
                        Data = null,
                    };
                }
                if (!user.IsActive)
                {
                    return new ResponseObject<DataResponseLogin>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Tài khoản bạn đang bị vô hiệu hóa",
                        Data = null,
                    };
                }

                
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đăng nhập thành công",
                    Data = new DataResponseLogin
                    {
                        AccessToken = GetJwtTokenAsync(user).Result.Data.AccessToken,
                        RefreshToken = GetJwtTokenAsync(user).Result.Data.RefreshToken,
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<ResponseObject<string>> LogoutAsync(string token)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Token không hợp lệ hoặc không tồn tại.",
                        Data = null,
                    };
                }
                if (!refreshToken.IsActive)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Token đã hết hạn hoặc đã bị hủy.",
                        Data = null,
                    };
                }
                refreshToken.IsActive = false;
                await _baseRefreshTokenRepository.UpdateAsync(refreshToken);
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đăng xuất thành công",
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ResponseObject<DataResponseUser>> Register(Request_Register request)
        {
            try
            {
                #region validCheck
                if (request.Username == null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Tài khoản không được để trống",
                        Data = null
                    };
                }
                if (request.PasswordHash == null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không được để trống",
                        Data = null
                    };
                }
                if (request.Email == null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email không được đê trống",
                        Data = null
                    };
                }
                if (!ValidateInput.IsValidEmail(request.Email))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status409Conflict,
                        Message = "Định dạng email không hợp lệ",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByEmail(request.Email) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email này đã tồn tại trên hệ thống",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByUserName(request.Username) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status409Conflict,
                        Message = "Tài khoản đã tồn tại",
                        Data = null
                    };
                }

                #endregion
                var user = new User
                {
                    Username = request.Username,
                    FullName = request.FullName,
                    PasswordHash = BcryptNet.HashPassword(request.PasswordHash),
                    Avatar = request.Avatar != null ? await HandleUploadFile.WirteFileAvatar(request.Avatar) : "default-image.png",
                    Email = request.Email,
                    CreatedAt = DateTime.Now,
                    IsActive = false,
                    
                };
                await _baseUserRepository.CreateAsync(user);
                await _userRepository.AddRolesToUserAsync(user, new List<string> { "User" });


                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    UserId = user.Id,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(3),
                    IsConfirm = false,
                };
                await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var emailContent = _emailService.GenerateConfirmationCodeEmail(confirmEmail.ConfirmCode);
                var message = new EmailMessage(new string[] { request.Email }, $"Mã xác nhận của {user.Username} ", emailContent);
                var responseMessage = _emailService.SendEmail(message);
                var responseUser = _userConverter.EntitytoDTO(user);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Đăng ký thành công! Vui lòng Nhận mã xác nhận tại email",
                    Data = responseUser
                };

            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null,
                };
            }
        }
        public async Task<ResponseObject<DataResponseLogin>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenInDb = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (tokenInDb == null || !tokenInDb.IsActive || tokenInDb.ExpiryTime <= DateTime.Now)
                {
                    return new ResponseObject<DataResponseLogin>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Refresh token không hợp lệ hoặc đã hết hạn",
                        Data = null
                    };
                }

                var user = await _baseUserRepository.GetAsync(u => u.Id == tokenInDb.UserId);
                if (user == null)
                {
                    return new ResponseObject<DataResponseLogin>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Người dùng không tồn tại",
                        Data = null
                    };
                }

                // Lấy claims và tạo lại AccessToken mới, giữ nguyên RefreshToken cũ
                var userRoles = await _baseUserRoleRepository.GetAllAsync(x => x.UserId == user.Id);
                var roles = await _baseRoleRepository.GetAllAsync();
                var authClaims = new List<Claim>
                    {
                        new Claim("Id", user.Id.ToString()),
                        new Claim("FullName", user.FullName),
                        new Claim("Username", user.Username),
                        new Claim("Email", user.Email)
                    };
                foreach (var userRole in userRoles)
                {
                    var role = roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                    if (role != null)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role.RoleCode));
                    }
                }

                var accessToken = GenerateAccessToken(authClaims);

                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Làm mới access token thành công",
                    Data = new DataResponseLogin
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken // vẫn giữ refresh token cũ
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi: " + ex.Message,
                    Data = null
                };
            }
        }


        #region Private Methods
        public static string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                                        .Select(s => s[random.Next(s.Length)])
                                        .ToArray());
        }
        private string GenerateCodeActive()
        {
            Random random = new Random();
            int code = random.Next(0, 100000); // Tạo số ngẫu nhiên từ 0 đến 99999
            return code.ToString("D5"); // Chuyển đổi thành chuỗi có độ dài 5, thêm '0' ở đầu nếu cần
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])),
                ValidateLifetime = false // Quan trọng: Không validate thời hạn của token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            var expiration = DateTime.Now.AddHours(tokenValidityInHours);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        private string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var jwtToken = GetToken(claims.ToList());
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }



        #endregion
    }
}
