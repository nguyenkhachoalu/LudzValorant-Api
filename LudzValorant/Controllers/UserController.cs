using LudzValorant.Constants;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudzValorant.Controllers
{
    [Route(Constant.DefaultValue.DEFAULTCONTROLLER_ROUTE)]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetPagedUsers(string? username, int pageNumber = 1, int pageSize = 10)
        {
            return Ok(await _userService.GetPagedUser(username, pageNumber, pageSize));
        }
        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid Id)
        {
            return Ok(await _userService.GetUserById(Id));
        }
        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateRoleOfUser(Guid id, List<string> roleCode)
        {
            return Ok(await _userService.UpdateRoleOfUser(id, roleCode));
        }
        [HttpGet]
        public async Task<IActionResult> GetRoleOfUser(Guid idUser)
        {
            return Ok(await _userService.GetRoleOfUser(idUser));
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetProfile()
        {
            Guid userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await _userService.GetUserById(userId));
        }
    }
}
