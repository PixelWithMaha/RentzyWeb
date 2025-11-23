using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Repositories;
using Rentzy.BLL.Services;
using Rentzy.DAL.Repository.Landlord;
using Rentzy.DAL.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<RentzyDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthService>();


builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ILandlordRepository, LandlordRepository>();

// Add Services
builder.Services.AddScoped<PropertyService>();

builder.Services.AddScoped<LandlordService>();


// ===== ADD SESSION CONFIGURATION HERE =====
builder.Services.AddDistributedMemoryCache(); // Required for session

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Security: cookie not accessible via JavaScript
    options.Cookie.IsEssential = true; // Required for GDPR compliance
    options.Cookie.Name = ".Rentzy.Session"; // Custom cookie name
});
// ==========================================

// Add HttpContextAccessor (optional, for accessing session in services)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== ADD SESSION MIDDLEWARE HERE (IMPORTANT ORDER!) =====
app.UseSession(); // Must be AFTER UseRouting() and BEFORE UseAuthorization()
// ========================================================

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();