using Microsoft.AspNetCore.Mvc;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Entity.Trendyol.Transaction;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Services;
using System.Diagnostics;

namespace Pazaryeri.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public TransactionController(ITransactionRepository transactionRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<TransactionController> logger,
            IConfiguration configuration)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }
        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("Transaction");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetTransactions()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var (transactions, totalCount) = await _transactionRepository.GetPagedAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = transactions.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        transactionId = o.TransactionId,
                        transactionType=o.TransactionType,
                        description=o.Description,
                        debt=o.Debt.ToString("C2"),
                        paymentDate=o.PaymentDate.ToString("dd.MM.yyyy HH:mm"),
                        actions = o.Id
                    })
                };

                return Json(returnObj);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FetchTrendyolTransactions()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                List<Transaction> transactions = await trendyolService.GetTransactionsAsync();
                int addedCount = 0;
                int updatedCount = 0;
                foreach (var transaction in transactions)
                {
                    var trans = await _transactionRepository.TransactionExistsAsync(transaction.TransactionId);

                    if (trans != null)
                    {
                        await _transactionRepository.UpdateAsync(trans);
                        updatedCount++;
                    }
                    else
                    {
                        await _transactionRepository.CreateAsync(transaction);
                        addedCount++;
                    }
                }
                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedCount} yeni muhasebe kaydı eklendi. {updatedCount} muhasebe kaydı güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol muhasebe kayıtları çekilirken hata: {ex.Message}"
                });
            }
        }

        private string GetPlatformDisplayName(Platform platform)
        {
            return platform switch
            {
                Platform.Trendyol => "Trendyol",
                _ => "Bilinmeyen"
            };
        }
    }
}
