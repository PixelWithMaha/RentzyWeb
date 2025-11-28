using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.Configuration;
using Rentzy.BLL.Services;
using Rentzy.BLL.Services.ApprovalServices;
using Rentzy.DAL.Context;
using Rentzy.DAL.Repositories;
using Rentzy.DAL.Repository.Approvals;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<RentzyDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Configure EmailSettings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddScoped<ILandlordApprovalService, LandlordApprovalService>();
builder.Services.AddScoped<ILandlordApprovalRepository, LandlordApprovalRepository>();


// Register services
builder.Services.AddScoped<AuthService>();

// ✅ Register Email Service (Switch between Mock and Real)
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
{
    builder.Services.AddScoped<EmailService>();
    Console.WriteLine("Using REAL Gmail Email Service");
}

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();