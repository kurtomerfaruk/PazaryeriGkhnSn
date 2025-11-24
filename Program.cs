using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPlatformServiceFactory, PlatformServiceFactory>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<TrendyolService>();

builder.Services.AddScoped<IOrderSyncService, OrderSyncService>();
builder.Services.AddHostedService<OrderSyncBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
