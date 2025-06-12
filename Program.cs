using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Hubs;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Setting;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notificationtype;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Report;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// 1. ĐỌC CẤU HÌNH TỪ ENV / appsettings
// ======================================
builder.Configuration.AddEnvironmentVariables();

// ======================================
// 2. KẾT NỐI DATABASE
// ======================================

//var dbConnection = builder.Configuration.GetConnectionString("DatabaseConnection")
//                 ?? Environment.GetEnvironmentVariable("DB_CONNECTION");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(dbConnection, sqlOptions =>
//        sqlOptions.EnableRetryOnFailure()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));



// ======================================
// 3. CẤU HÌNH IDENTITY
// ======================================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ======================================
// 4. CẤU HÌNH COOKIE & SESSION
// ======================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ======================================
// 5. CẤU HÌNH MVC, SIGNALR, RAZOR
// ======================================
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

// ======================================
// 6. ĐĂNG KÝ CÁC DỊCH VỤ RIÊNG
// ======================================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<PaymentReminderService>();
builder.Services.AddHostedService<AttendanceReminderService>();
builder.Services.AddScoped<ITrafficService, TrafficService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Cấu hình Email từ appsettings.json hoặc ENV
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// ======================================
// 7. BUILD APP
// ======================================
var app = builder.Build();

// ======================================
// 8. SEED DỮ LIỆU BAN ĐẦU
// ======================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    await SeedDataAsync(context, userManager, roleManager);
}

// ======================================
// 9. CẤU HÌNH MIDDLEWARE
// ======================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ======================================
// 10. SEED ROLE, USER, PHÒNG HỌC
// ======================================
async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    string[] roles = new[] { "SuperAdmin", "Admin", "Teacher", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role, NormalizedName = role.ToUpper() });
        }
    }

    if (!await userManager.Users.AnyAsync())
    {
        var admin = new ApplicationUser
        {
            UserName = "ntt112004h@gmail.com",
            Email = "ntt112004h@gmail.com",
            FullName = "System Administrator",
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Thinh123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "SuperAdmin");
        }
    }

    //if (!await context.Rooms.AnyAsync())
    //{
    //    var rooms = new List<Room>
    //    {
    //        new Room { Name = "Room 101", Code = "R101", Capacity = 20, Type = RoomType.Classroom, IsActive = true },
    //        new Room { Name = "Computer Lab 1", Code = "CL01", Capacity = 25, Type = RoomType.ComputerLab, IsActive = true },
    //        new Room { Name = "Audio Lab", Code = "AL01", Capacity = 15, Type = RoomType.AudioLab, IsActive = true }
    //    };

    //    context.Rooms.AddRange(rooms);
    //    await context.SaveChangesAsync();
    //}
}
