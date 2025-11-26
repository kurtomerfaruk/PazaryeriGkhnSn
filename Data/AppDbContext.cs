using Microsoft.EntityFrameworkCore;
using Pazaryeri.Models;

namespace Pazaryeri.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<TrendyolOrderDetail> TrendyolOrderDetails => Set<TrendyolOrderDetail>(); 
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductVariantImage> ProductVariantImages => Set<ProductVariantImage>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
        public DbSet<CategoryAttributeValue> CategoryAttributeValue => Set<CategoryAttributeValue>();
        public DbSet<TrendyolAttribute> TrendyolAttributes => Set<TrendyolAttribute>();
        public DbSet<TrendyolImage> TrendyolImages => Set<TrendyolImage>();
        public DbSet<TrendyolProductDetail> TrendyolProductDetails => Set<TrendyolProductDetail>();
        public DbSet<TrendyolRejectReasonDetail> TrendyolRejectReasonDetails => Set<TrendyolRejectReasonDetail>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
     
    }
}
