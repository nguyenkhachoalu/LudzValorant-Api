using LudzValorant.Constants;
using LudzValorant.Services.ImplementServices;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudzValorant.Controllers
{
    [Route(Constant.DefaultValue.DEFAULTCONTROLLER_ROUTE)]
    [ApiController]
    [Authorize]
    public class SkinImportController : Controller
    {
        private readonly ISkinImporterService _skinImporterService;

        public SkinImportController(ISkinImporterService skinImporterService)
        {
            _skinImporterService = skinImporterService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportAll()
        {
            await _skinImporterService.ImportAllSkinsAsync();
            return Ok(new { message = "Skin import completed successfully." });
        }

        [HttpPost("from-riot")]
        public async Task<IActionResult> GetAccountFromRiotUrl([FromBody] string riotRedirectUrl)
        {
            var result = await _skinImporterService.GetAccountByUrlRiot(riotRedirectUrl);
            return Ok(result);
        }

    }
}
