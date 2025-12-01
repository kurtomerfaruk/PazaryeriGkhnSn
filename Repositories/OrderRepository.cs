using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
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

        public async Task<Models.Order> OrderExistsAsync(string orderNumber, Platform platform)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.Platform == platform);
        }

        public async Task<(List<Models.Order> Items, int TotalCount)> GetPagedAsync(int start, int length, string search, string sortColumn, string sortDirection)
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
                .Include(o=>o.TrendyolDetails)
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

    }
}
