using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IClaimRepository:IRepository<Claim>
    {
        Task AddOrUpdateRangeAsync(List<Models.Claim> claims);
        Task<Claim> GetWithDetailsAsync(int id);
    }
}
