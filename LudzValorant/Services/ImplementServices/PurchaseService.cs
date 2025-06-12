using LudzValorant.Entities;
using LudzValorant.Enums;
using LudzValorant.Payloads.Request.PurchaseRequests;
using LudzValorant.Payloads.ResponseModels.DataPurchase;
using LudzValorant.Payloads.Responses;
using LudzValorant.Repositories.InterfaceRepositories;
using LudzValorant.Services.InterfaceServices;
using Microsoft.EntityFrameworkCore;

namespace LudzValorant.Services.ImplementServices
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IBaseRepository<Purchase> _basePurchaseRepository;
        private readonly IBaseRepository<InstallmentSchedule> _baseInstallmentsRepository;
        private readonly IBaseRepository<Product> _baseProductRepository;
        private readonly IPurchaseRepository _purchaseRepository;

        public PurchaseService(IBaseRepository<Purchase> basePurchaseRepository, IBaseRepository<InstallmentSchedule> baseInstallmentsRepository, IBaseRepository<Product> baseProductRepository, IPurchaseRepository purchaseRepository)
        {
            _basePurchaseRepository = basePurchaseRepository;
            _baseInstallmentsRepository = baseInstallmentsRepository;
            _baseProductRepository = baseProductRepository;
            _purchaseRepository = purchaseRepository;
        }

        public async Task<ResponsePagedResult<DataResponsePagedPurchase>> GetPagedPurchaseByUserId(
            Guid userId,
            PurchaseStatus? status,
            PaymentMethod? paymentMethod,
            int pageNumber,
            int pageSize)
        {
            var pagedResult = await _purchaseRepository.GetPagedPurchasesByUserIdAsync(
                userId, status, paymentMethod, pageNumber, pageSize);

            var items = pagedResult.Items.Select(p => new DataResponsePagedPurchase
            {
                Id = p.Id,
                ProductId = p.ProductId,
                Title = p.Product.Title,
                Status = p.Status,
                TotalAmount = p.TotalAmount,
                PaymentMethod = p.PaymentMethod,
                ContactInfo = p.ContactInfo,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return new ResponsePagedResult<DataResponsePagedPurchase>
            {
                Items = items,
                TotalItems = pagedResult.TotalItems,
                TotalPages = pagedResult.TotalPages,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }


        public async Task<ResponseObject<DataResponsePurchase>> GetPurchaseById(Guid id)
        {
            var purchase = await _basePurchaseRepository.GetByIdAsync(id);
            if (purchase == null)
            {
                return new ResponseObject<DataResponsePurchase>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy thông tin mua hàng",
                    Data = null
                };
            }

            var installmentSchedules = await _baseInstallmentsRepository.GetAllAsync(i => i.PurchaseId == id);
            var scheduleList = await installmentSchedules.ToListAsync();

            var response = new DataResponsePurchase
            {
                Id = purchase.Id,
                ProductId = purchase.ProductId,
                PaymentMethod = purchase.PaymentMethod,
                Status = purchase.Status,
                TotalAmount = purchase.TotalAmount,
                Note = purchase.Note,
                ContactInfo = purchase.ContactInfo,
                CreatedAt = purchase.CreatedAt,
                UpdatedAt = purchase.UpdatedAt,
                installmentSchedules = scheduleList.Select(s => new DataResponseInstallmentSchedule
                {
                    Id = s.Id,
                    PurchaseId = s.PurchaseId,
                    DueDate = s.DueDate,
                    Amount = s.Amount,
                    IsPaid = s.IsPaid,
                    PaidAt = s.PaidAt,

                }).ToList()
            };

            return new ResponseObject<DataResponsePurchase>
            {
                Status = StatusCodes.Status200OK,
                Message = "Lấy thông tin mua hàng thành công",
                Data = response
            };
        }


        public async Task<ResponseObject<string>> Purchase(PurchaseRequest request)
        {
            var product = await _baseProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Sản phẩm không tồn tại",
                    Data = null
                };
            }

            var purchasesByProduct = await _basePurchaseRepository.GetAllAsync(x => x.ProductId == request.ProductId);
            var purchaseList = await purchasesByProduct.ToListAsync();

            if (purchaseList.Any(p => p.Status != PurchaseStatus.CANCELLED))
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Sản phẩm này đã được mua hoặc đang trong quá trình trả góp.",
                    Data = null
                };
            }

            var purchase = new Purchase
            {
                ProductId = request.ProductId,
                PaymentMethod = request.PaymentMethod,
                Status = PurchaseStatus.COMPLETED ,
                TotalAmount = request.TotalAmount,
                Note = request.Note,
                ContactInfo = request.ContactInfo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            List<InstallmentSchedule> installmentSchedules = null;

            if (purchase.PaymentMethod == PaymentMethod.INSTALLMENT)
            {
                installmentSchedules = new List<InstallmentSchedule>();

                // Số tiền còn lại sau thanh toán đầu tiên
                decimal remainingAmount = request.TotalAmount - request.AmountOfFirstPayment;
                int periods = request.InstallmentPeriod;

                // Tính số tiền cho mỗi kỳ
                decimal perPeriodAmount = Math.Round(remainingAmount / periods, 2);

                // Tạo lịch cho lần đầu tiên (đã thanh toán)
                installmentSchedules.Add(new InstallmentSchedule
                {
                    Amount = request.AmountOfFirstPayment,
                    IsPaid = true,
                    PaidAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow,
                    Purchase = purchase
                });

                // Các kỳ trả góp tiếp theo
                for (int i = 1; i <= periods; i++)
                {
                    installmentSchedules.Add(new InstallmentSchedule
                    {
                        Amount = perPeriodAmount,
                        IsPaid = false,
                        DueDate = DateTime.UtcNow.AddMonths(i),
                        Purchase = purchase
                    });
                }

                purchase.Installments = installmentSchedules;
            }

            // Lưu Purchase vào database
            await _basePurchaseRepository.CreateAsync(purchase);

            // Nếu có trả góp thì lưu danh sách InstallmentSchedule
            if (installmentSchedules != null && installmentSchedules.Any())
            {
                await _baseInstallmentsRepository.CreateAsync(installmentSchedules);
            }

            return new ResponseObject<string>
            {
                Status = StatusCodes.Status201Created,
                Message = "Giao dịch mua sản phẩm đã được tạo thành công", 
                Data = "Mua thành công"
            };
        }

        public async Task<ResponseObject<string>> UpdatePurchase(Guid purchaseId,PurchaseUpdateRequest request)
        {
            var purchase = await _basePurchaseRepository.GetByIdAsync(purchaseId );
            if (purchase == null)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy giao dịch mua cần cập nhật",
                    Data = null
                };
            }

            purchase.TotalAmount = request.TotalAmount;
            purchase.Note = request.Note;
            purchase.ContactInfo = request.ContactInfo;
            purchase.UpdatedAt = DateTime.UtcNow;

            await _basePurchaseRepository.UpdateAsync(purchase);

            return new ResponseObject<string>
            {
                Status = StatusCodes.Status200OK,
                Message = "Cập nhật giao dịch mua thành công",
                Data = "Thành công"
            };
        }


    }
}
