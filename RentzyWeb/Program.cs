using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.Configuration;
using Rentzy.BLL.Services;
using Rentzy.BLL.Services.ApprovalServices;
using Rentzy.BLL.Services.ReportsServices;
using Rentzy.DAL.Context;
using Rentzy.DAL.Repositories;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Repository.Approvals;
using Rentzy.DAL.Repository.Landlord;
using Rentzy.DAL.Repository.Reports;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllersWithViews();

// Booking
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// DbContext
builder.Services.AddSession();


// Add DbContext
builder.Services.AddDbContext<RentzyDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// User repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Landlord Approvals
builder.Services.AddScoped<ILandlordApprovalService, LandlordApprovalService>();
builder.Services.AddScoped<ILandlordApprovalRepository, LandlordApprovalRepository>();

builder.Services.AddScoped<IUserStatuses_service, UserStatuses_service>();
builder.Services.AddScoped<IUserStatuses_Repo, UserStatuses_Repo>();


builder.Services.AddScoped<IPropertyApprovalRequestsRepo, PropertyApprovalRequestsRepo>();
builder.Services.AddScoped<IPropertyApprovalRequestService, PropertyApprovalRequestService>();

// Add Services
// Payment
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

builder.Services.AddScoped<ILandlordRepository, LandlordRepository>();
builder.Services.AddScoped<LandlordService>();
builder.Services.AddScoped<PropertyApprovalRequestService>();

// Auth Service
builder.Services.AddScoped<AuthService>();

// Add these inside builder.Services
builder.Services.AddScoped<IPaymentNotificationRepository, PaymentNotificationRepository>();
builder.Services.AddScoped<IRentalRequestRepository, RentalRequestRepository>();

builder.Services.AddScoped<ITenantBookingService, TenantBookingService>();


// Your existing services
builder.Services.AddScoped<RentalRequestService>();
builder.Services.AddScoped<PaymentNotificationService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<TenantPaymentService>();
builder.Services.AddScoped<TenantBookingService>();

// Email Service
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
builder.Services.AddScoped<EmailService>();
Console.WriteLine("Using REAL Gmail Email Service");

// Reports
builder.Services.AddScoped<ReportsRepository>();
builder.Services.AddScoped<ReportsService>();

// Reviews
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ReviewService>();

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
app.UseSession();

// Middleware
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

// ==========================================
//  AUTO-SEEDER: Ensure lookup data exists
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RentzyDBContext>();
        var dbCreated = context.Database.CanConnect();
        
        if (dbCreated)
        {
            bool modified = false;

            // ==========================================
            // SEEDING CORE DATA
            // ==========================================
            context.Database.OpenConnection();
            
            try 
            {
                // Verify Admin User
                var existingAdmin = context.Users.FirstOrDefault(u => u.Email == "admin@rentzy.com");
                if (existingAdmin == null)
                {
                    Console.WriteLine(">>> CREATING NEW ADMIN USER...");
                    var adminUser = new Rentzy.DAL.Models.Admin
                    {
                        FirstName = "System",
                        LastName = "Admin",
                        Email = "admin@rentzy.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 8),
                        Phone = "0000000000",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                    Console.WriteLine(">>> Default Admin user created successfully (admin@rentzy.com / Admin123!).");
                    existingAdmin = adminUser;
                }
                else
                {
                    Console.WriteLine(">>> ADMIN USER DETECTED. Resetting password hash to confirm match...");
                    existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 8);
                    context.SaveChanges();
                    Console.WriteLine(">>> Admin password hash forcibly synchronized.");
                }

                // DIAGNOSTIC CHECK
                Console.WriteLine($">>> DIAGNOSTIC: Admin Email in DB: '{existingAdmin.Email}'");
                Console.WriteLine($">>> DIAGNOSTIC: Admin Hash in DB: '{existingAdmin.PasswordHash}'");
                bool isValid = BCrypt.Net.BCrypt.Verify("Admin123!", existingAdmin.PasswordHash);
                Console.WriteLine($">>> DIAGNOSTIC: Verify 'Admin123!' against DB hash: {isValid}");
            } 
            catch (Exception ex)
            {
                Console.WriteLine($">>> ERROR INITIALIZING ADMIN: {ex.Message}");
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> Warning: Failed to initialize lookup seeds on startup: {ex.Message}");
    }
}

app.Run();
