using LudzValorant.Constants;
using LudzValorant.Enums;
using LudzValorant.Payloads.Request.AccountRequests;
using LudzValorant.Payloads.Request.ProductRequests;
using LudzValorant.Services.ImplementServices;
using LudzValorant.Services.InterfaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudzValorant.Controllers
{
    [Route(Constant.DefaultValue.DEFAULTCONTROLLER_ROUTE)]
    [ApiController]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IAccountService _accountService;
        private readonly ISkinImporterService _skinImporterService;

        public ProductController(IProductService productService, IAccountService accountService, ISkinImporterService skinImporterService)
        {
            _productService = productService;
            _accountService = accountService;
            _skinImporterService = skinImporterService;
        }


        /// <summary>
        /// Thêm sản phẩm mới
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateControRequest request)
        {
            var getInforAccount = await _skinImporterService.GetAccountByUrlRiot(request.AccountRequest.Url);
            if (getInforAccount.Status != StatusCodes.Status200OK)
            {
                return BadRequest(getInforAccount.Message);
            }
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var accResult = await _accountService.AddAccount(userId, request.AccountRequest.ExpireTime, getInforAccount.Data);
            if(accResult.Status != StatusCodes.Status201Created)
            {
                return BadRequest(accResult.Message);

            }
            var productRequest = new ProductRequest
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Image = request.image,
                AccountId = accResult.Data,
            };
            var result = await _productService.AddProductAsync(userId, productRequest);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm của user (toàn bộ - không phân trang)
        /// </summary>
        [HttpGet("owner/all")]
        public async Task<IActionResult> GetAllProductsByOwner()
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _productService.GetAllProductsByOwnerId(userId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm của user theo phân trang và từ khóa
        /// </summary>
        [HttpGet("owner/paged")]
        public async Task<IActionResult> GetPagedProductsByOwner([FromQuery] ProductFilterRequest request)
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _productService.GetPagedProductsByOwnerId(userId, request);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm phân trang và tìm kiếm
        /// </summary>
        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPagedProducts([FromQuery] ProductFilterRequest request)
        {
            var result = await _productService.GetPagedProducts(request);
            return Ok(result);
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        [HttpDelete("{productId:guid}")]
        public async Task<IActionResult> DeleteProduct(Guid productId)
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _productService.DeletedProduct(userId, productId);
            return Ok(result);
        }

        /// <summary>
        /// Chỉnh sửa thông tin sản phẩm
        /// </summary>
        [HttpPut("edit")]
        public async Task<IActionResult> EditProduct([FromForm] UpdateProductRequest request)
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("Id").Value);
            var result = await _productService.EditProduct(userId, request);
            return Ok(result);
        }
        /// <summary>
        /// Lấy chi tiết sản phẩm theo ID
        /// </summary>
        [HttpGet("{productId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(Guid productId, bool? filterTier = false)
        {
            var result = await _productService.GetProductByIdAsync(productId, filterTier);
            return Ok(result);
        }


    }
}
