using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class OrderRepository:IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> OrderExistsAsync(string orderNumber, OrderPlatform platform)
        {
            return await _context.Orders
                .AnyAsync(o => o.OrderNumber == orderNumber && o.Platform == platform);
        }

        public async Task<(List<Models.Order> Orders, int TotalCount)> GetPagedOrdersAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    o.CustomerName.Contains(search) ||
                    o.CustomerEmail.Contains(search) ||
                    o.Status.Contains(search) ||
                    o.Platform.ToString().Contains(search));
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(GetSortProperty(sortColumn))
                    : query.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                query = query.OrderByDescending(o => o.OrderDate);
            }

            var orders = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (orders, totalCount);
        }

        private static Expression<Func<Models.Order, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "ordernumber" => order => order.OrderNumber,
                "customername" => order => order.CustomerName,
                "price" => order => order.TotalPrice,
                "status" => order => order.Status,
                "platform" => order => order.Platform,
                "orderdate" => order => order.OrderDate,
                _ => order => order.Id
            };
        }

        public async Task<List<Models.Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Models.Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o=>o.Id==id);
        }

        public async Task<Models.Order> CreateAsync(Models.Order siparis)
        {
            _context.Orders.Add(siparis);
            await _context.SaveChangesAsync();
            return siparis;
        }

        public async Task<Models.Order> UpdateAsync(Models.Order siparis)
        {
            _context.Orders.Update(siparis);
            await _context.SaveChangesAsync();
            return siparis;
        }

        public async Task DeleteAsync(int id)
        {
            var siparis = await GetByIdAsync(id);
            if (siparis != null)
            {
                _context.Orders.Remove(siparis);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Models.Order> GetWithDetailsAsync(int id)
        {
            return await _context.Orders
             .Include(o => o.TrendyolDetails)
             .FirstOrDefaultAsync(o => o.Id == id);
        }

        public Task<TrendyolOrderDetail> GetTrendyolOrderDetailAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        //public async Task<Models.Order> GetWithDetailsAsync(int id)
        //{
        //    return await _context.Siparisler
        //        .Include(o => o.TrendyolSiparisDetay)
        //        .FirstOrDefaultAsync(o => o.Id == id);
        //}

        //public async Task<List<Models.Order>> GetOrdersByPlatformAsync(OrderPlatform platform)
        //{
        //    return await _context.Siparisler
        //        .Include(o => o.TrendyolSiparisDetay)
        //        .Where(o => o.Platform == platform)
        //        .ToListAsync();
        //}

        //public async Task SaveManualOrderAddresses(int orderId, Address invoiceAddress, Address shipmentAddress)
        //{
        //    var order = await _context.Siparisler.FindAsync(orderId);
        //    if (order != null)
        //    {
        //        order.InvoiceAddressJson = JsonHelper.Serialize(invoiceAddress);
        //        order.ShipmentAddressJson = JsonHelper.Serialize(shipmentAddress);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        //// Trendyol detayları getir
        //public async Task<TrendyolOrderDetail> GetTrendyolOrderDetailAsync(int orderId)
        //{
        //    return await _context.TrendyolSiparisDetays
        //        .FirstOrDefaultAsync(t => t.OrderId == orderId);
        //}
    }
}
