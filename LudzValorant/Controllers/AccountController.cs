using LudzValorant.Constants;
using LudzValorant.Payloads.Request.AccountRequests;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudzValorant.Controllers
{
    [Route(Constant.DefaultValue.DEFAULTCONTROLLER_ROUTE)]
    [ApiController]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ISkinImporterService _skinImporterService;

        public AccountController(IAccountService accountService, ISkinImporterService skinImporterService)
        {
            _accountService = accountService;
            _skinImporterService = skinImporterService;
        }


        /// <summary>
        /// Thêm tài khoản Valorant mới cho user (đã có thông tin đầy đủ)
        /// </summary>
        [HttpPost("add")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddAccount([FromBody] AccountRequest request)
        {
            
            var getInforAccount = await _skinImporterService.GetAccountByUrlRiot(request.Url);
            if(getInforAccount.Status != StatusCodes.Status200OK)
            {
                return BadRequest(getInforAccount.Message);
            }
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _accountService.AddAccount(userId, request.ExpireTime, getInforAccount.Data);
            return Ok(result);
        }

        /// <summary>
        /// Lấy tài khoản Valorant theo userId đang đăng nhập
        /// </summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAccountByUser()
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _accountService.GetAccountByUserId(userId);
            return Ok(result);
        }



        [HttpGet("GetById")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAccountById([FromQuery] Guid Id, bool? filterTier = false)
        {
            var result = await _accountService.GetAccountById(Id, filterTier);
            return Ok(result);
        }
        /// <summary>
        /// Lấy tài khoản từ Riot URL redirect trả về
        /// </summary>

    }
}
