using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Handle.HandleFile;
using LudzValorant.Payloads.Request.ProductRequests;
using LudzValorant.Payloads.ResponseModels.DataProduct;
using LudzValorant.Payloads.Responses;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.InterfaceServices;

namespace LudzValorant.Services.ImplementServices
{
    public class ProductService : IProductService
    {
        private readonly IBaseRepository<Product> _baseProductRepository;
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAccountService _accountService;
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductService(IBaseRepository<Product> baseProductRepository, IBaseRepository<User> baseUserRepository, IUserRepository userRepository, IAccountService accountService, IProductRepository productRepository, IHttpContextAccessor httpContextAccessor)
        {
            _baseProductRepository = baseProductRepository;
            _baseUserRepository = baseUserRepository;
            _userRepository = userRepository;
            _accountService = accountService;
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObject<string>> AddProductAsync(Guid userId, ProductRequest request)
        {
            try
            {
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Người dùng không tồn tại",
                        Data = null
                    };
                }
                var roles = await _userRepository.GetRolesOfUserAsync(user);
                if (!roles.Any(r => r == "ADMIN" || r == "MOD"))
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Người dùng không có quyền thêm sản phẩm",
                        Data = null
                    };

                }
                    var product = new Product
                {
                    AccountId = request.AccountId,
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    OwnerId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Image = request.Image != null ?  await HandleUploadFile.WirteFileProduct(request.Image) : "default-image.png",
                    IsPublic = true,
                };

                await _baseProductRepository.CreateAsync(product);
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Sản phẩm đã được thêm thành công",
                    Data = "thành công",
                };

            }
            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Lỗi hệ thống: " + ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ResponseObject<string>> DeletedProduct(Guid userId, Guid productId)
        {
            var user = await _baseUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Người dùng không tồn tại",
                    Data = null
                };
            }
            var roles = await _userRepository.GetRolesOfUserAsync(user);
            if (!roles.Any(r => r == "ADMIN" || r == "MOD"))
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = "Người dùng không có quyền xóa sản phẩm này",
                    Data = null
                };

            }
            var product = await _baseProductRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Sản phẩm không tồn tại",
                    Data = null
                };
            }
            if (!roles.Contains("ADMIN"))
            {
                if(product.OwnerId != user.Id)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Người dùng không có quyền xóa sản phẩm không phải của mình",
                        Data = null
                    };
                }
            }
            var deletedAccount = await _accountService.DeleteAccountById(product.AccountId);
            if (deletedAccount.Status != StatusCodes.Status200OK)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = deletedAccount.Message,
                    Data = null
                };
            }
            await _baseProductRepository.DeleteAsync(productId);
            return new ResponseObject<string>
            {
                Status = StatusCodes.Status200OK,
                Message = "Sản phẩm đã được xóa thành công",
                Data = "thành công",
            };
        }

        public async Task<ResponseObject<string>> EditProduct(Guid userId, UpdateProductRequest request)
        {
            var user = await _baseUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Người dùng không tồn tại",
                    Data = null
                };
            }
            var roles = await _userRepository.GetRolesOfUserAsync(user);
            if (!roles.Any(r => r == "ADMIN" || r == "MOD"))
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = "Người dùng không có quyền xóa sản phẩm này",
                    Data = null
                };

            }
            var product = await _baseProductRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Sản phẩm không tồn tại",
                    Data = null
                };
            }
            if (!roles.Contains("ADMIN"))
            {
                if (product.OwnerId != user.Id)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Người dùng không có quyền xóa sản phẩm không phải của mình",
                        Data = null
                    };
                }
            }
            product.Title = request.Title;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsPublic = request.IsPublic;
            await _baseProductRepository.UpdateAsync(product);
            return new ResponseObject<string>
            {
                Status = StatusCodes.Status200OK,
                Message = "Sản phẩm đã được sửa thành công",
                Data = "thành công",
            };
        }

       
        public async Task<ResponseObject<DataResponseProductWithDetails>> GetProductByIdAsync(Guid productId, bool? filterTier)
        {
            var product = await _baseProductRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return new ResponseObject<DataResponseProductWithDetails>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Sản phẩm không tồn tại",
                    Data = null
                };
            }

            var account = await _accountService.GetAccountById(product.AccountId, filterTier);
            if (account.Status != StatusCodes.Status200OK)
            {
                return new ResponseObject<DataResponseProductWithDetails>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Tài khoản gắn với sản phẩm không tồn tại",
                    Data = null
                };
            }

            string imageUrls = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/images/product/{product.Image}";

            var productDetails = new DataResponseProductWithDetails
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                OwnerId = product.OwnerId,
                OwnerName = product.Owner.Username,
                Price = product.Price,
                CreatedAt = product.CreatedAt,
                IsPublic = product.IsPublic,
                Image = imageUrls,
                
                Account = account.Data
            };

            return new ResponseObject<DataResponseProductWithDetails>
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy chi tiết sản phẩm thành công",
                Data = productDetails
            };
        }
        public async Task<ResponseObject<IEnumerable<DataResponseProduct>>> GetAllProductsByOwnerId(Guid ownerId)
        {
            var user = await _baseUserRepository.GetByIdAsync(ownerId);
            if (user == null)
            {
                return new ResponseObject<IEnumerable<DataResponseProduct>>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Người dùng không tồn tại",
                    Data = null
                };
            }
            var roles = await _userRepository.GetRolesOfUserAsync(user);
            if (!roles.Any(r => r == "ADMIN" || r == "MOD"))
            {
                return new ResponseObject<IEnumerable<DataResponseProduct>>
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = "Người dùng không có quyền xóa sản phẩm này",
                    Data = null
                };
            }
            var products = await _baseProductRepository.GetAllAsync(p => p.OwnerId == ownerId);
            if (!products.Any())
            {
                return new ResponseObject<IEnumerable<DataResponseProduct>>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không có sản phẩm nào của người dùng này",
                    Data = null
                };
            }

            var productList = new List<DataResponseProduct>();

            foreach (var product in products)
            {
                var account = await _accountService.GetAccountById(product.AccountId);
                if (account.Status != StatusCodes.Status200OK) continue;

                var item = new DataResponseProduct
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    OwnerId = product.OwnerId,
                    OwnerName = product.Owner.Username,
                    Price = product.Price,
                    CreatedAt = product.CreatedAt,
                    AccountId = product.AccountId,
                    IsPublic = product.IsPublic,
                    Image = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/images/product/{product.Image}"

                };
                productList.Add(item);
            }
            return new ResponseObject<IEnumerable<DataResponseProduct>>
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy danh sách sản phẩm thành công",
                Data = productList,
            };
        }
        public async Task<ResponsePagedResult<DataResponseProduct>> GetPagedProducts(string? keyword, ProductSearchType? productSearchType, bool? isPublic,int pageNumber, int pageSize)
        {
            var pagedProducts = await _productRepository.GetPagedProductsBySkinNameAsync(null, keyword, productSearchType, isPublic, pageNumber, pageSize);

            var productList = new List<DataResponseProduct>();

            foreach (var product in pagedProducts.Items)
            {
                var account = await _accountService.GetAccountById(product.AccountId);

                if (account.Status != StatusCodes.Status200OK) continue;

                productList.Add(new DataResponseProduct
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    OwnerId = product.OwnerId,
                    OwnerName = product.Owner.Username,
                    Price = product.Price,
                    CreatedAt = product.CreatedAt,
                    AccountId = product.AccountId,
                    IsPublic = product.IsPublic,
                    Image = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/images/product/{product.Image}"

                });
            }

            return new ResponsePagedResult<DataResponseProduct>
            {
                Items = productList,
                TotalItems = pagedProducts.TotalItems,
                TotalPages = pagedProducts.TotalPages,
                PageNumber = pagedProducts.PageNumber,
                PageSize = pagedProducts.PageSize
            };
        }

        public async Task<ResponsePagedResult<DataResponseProduct>> GetPagedProductsByOwnerId(Guid ownerId, string? keyword, ProductSearchType? productSearchType, bool? isPublic, int pageNumber, int pageSize)
        {
            var user = await _baseUserRepository.GetByIdAsync(ownerId);
            if (user == null)
            {
                return new ResponsePagedResult<DataResponseProduct>
                {
                    Items = null,
                    TotalItems = 0,
                    TotalPages = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var pagedProducts = await _productRepository.GetPagedProductsBySkinNameAsync(ownerId, keyword, productSearchType, isPublic, pageNumber, pageSize);

            var productList = new List<DataResponseProduct>();

            foreach (var product in pagedProducts.Items)
            {
                var account = await _accountService.GetAccountById(product.AccountId);

                if (account.Status != StatusCodes.Status200OK) continue;

                productList.Add(new DataResponseProduct
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    OwnerId = product.OwnerId,
                    OwnerName = product.Owner.Username,
                    Price = product.Price,
                    CreatedAt = product.CreatedAt,
                    AccountId = product.AccountId,
                    IsPublic = product.IsPublic,
                    Image = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/images/product/{product.Image}"
                });
            }

            return new ResponsePagedResult<DataResponseProduct>
            {
                Items = productList,
                TotalItems = pagedProducts.TotalItems,
                TotalPages = pagedProducts.TotalPages,
                PageNumber = pagedProducts.PageNumber,
                PageSize = pagedProducts.PageSize
            };
        }

    }

}
