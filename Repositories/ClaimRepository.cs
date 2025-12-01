using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Claim> CreateAsync(Claim entity)
        {
            _context.Claims.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var claim = await GetByIdAsync(id);
            if (claim != null)
            {
                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        public async Task<Claim> GetByIdAsync(int id)
        {
            return await _context.Claims.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Claim> Items, int TotalCount)> GetPagedAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Claims.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
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
                query = query.OrderByDescending(o => o.Id);
            }

            var claims = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (claims, totalCount);
        }

        private static Expression<Func<Claim, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => category => category.Id,
                "platform" => category => category.Platform,
                _ => category => category.Id
            };
        }

        public async Task<Claim> UpdateAsync(Claim entity)
        {
            _context.Claims.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task AddOrUpdateRangeAsync(List<Claim> claims)
        {
            foreach (var claim in claims)
            {

                var existingClaim = await _context.Claims
                    .Include(a => a.Trendyols)
                    .FirstOrDefaultAsync(a => a.TrendyolClaimId == claim.TrendyolClaimId);

                if (existingClaim == null)
                {
                    _context.Claims.Add(claim);
                }
                else
                {
                    existingClaim.TrendyolClaimId = claim.TrendyolClaimId;
                    existingClaim.OrderNumber = claim.OrderNumber;
                    existingClaim.OrderDate = claim.OrderDate;
                    existingClaim.CustomerName = claim.CustomerName;
                    existingClaim.ClaimDate = claim.ClaimDate;
                    existingClaim.CargoTrackingNumber = claim.CargoTrackingNumber;
                    existingClaim.CargoName = claim.CargoName;
                    existingClaim.OrderShipmentPackageId = claim.OrderShipmentPackageId;
                    existingClaim.LastModifiedDate = claim.LastModifiedDate;


                    foreach (var val in claim.Trendyols)
                    {

                        var existingVal = existingClaim.Trendyols.FirstOrDefault(v => v.TrendyolClaimId == val.TrendyolClaimId);

                        if (existingVal == null)
                        {
                            existingClaim.Trendyols.Add(val);
                        }
                        else
                        {
                            existingVal.TrendyolClaimId=val.TrendyolClaimId;
                            existingVal.Items = val.Items;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Models.Claim> GetWithDetailsAsync(int id)
        {
            return await _context.Claims
             .Include(o => o.Trendyols)
             .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
