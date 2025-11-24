using Microsoft.EntityFrameworkCore;
using Pazaryeri.Models;

namespace Pazaryeri.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<TrendyolOrderDetail> TrendyolOrderDetails => Set<TrendyolOrderDetail>();    
     
    }
}
