using LudzValorant.Constants;
using LudzValorant.Enums;
using LudzValorant.Payloads.Request.PurchaseRequests;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LudzValorant.Controllers
{
    [Route(Constant.DefaultValue.DEFAULTCONTROLLER_ROUTE)]
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        /// <summary>
        /// Mua sản phẩm (thanh toán hoặc trả góp)
        /// </summary>
        [HttpPost("buy")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseRequest request)
        {
            var result = await _purchaseService.Purchase(request);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin giao dịch
        /// </summary>
        [HttpPut("{purchaseId:guid}/update")]
        public async Task<IActionResult> UpdatePurchase(Guid purchaseId, [FromBody] PurchaseUpdateRequest request)
        {
            var result = await _purchaseService.UpdatePurchase(purchaseId, request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch của người dùng (phân trang và lọc)
        /// </summary>
        [HttpGet("user/paged")]
        public async Task<IActionResult> GetPagedPurchaseByUser([FromQuery] PurchaseStatus? status, [FromQuery] PaymentMethod? paymentMethod, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _purchaseService.GetPagedPurchaseByUserId(userId, status, paymentMethod, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết giao dịch theo ID
        /// </summary>
        [HttpGet("{purchaseId:guid}")]
        public async Task<IActionResult> GetPurchaseById(Guid purchaseId)
        {
            var result = await _purchaseService.GetPurchaseById(purchaseId);
            return Ok(result);
        }
    }
}
