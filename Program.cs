using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Hubs;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Facilities;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Background;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Email;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Notificationtype;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Reponsitory.Traffic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================
// 1. CONFIGURE DATABASE
// ============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

// ============================
// 2. CONFIGURE IDENTITY
// ============================
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


// ============================
// 3. CONFIGURE COOKIES
// ============================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ============================
// 4. ADD SERVICES
// ============================
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Custom Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<PaymentReminderService>();
builder.Services.AddHostedService<AttendanceReminderService>();
builder.Services.AddScoped<ITrafficService, TrafficService>();


// ============================
// 5. BUILD APP
// ============================
var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    await SeedDataAsync(context, userManager, roleManager);
}
// ============================
// 6. CONFIGURE MIDDLEWARE
// ============================
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


// =========================
//  SEED METHOD
// =========================
async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    // Seed roles if they don't exist
    if (!await roleManager.RoleExistsAsync("SuperAdmin"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" });
    }

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN" });
    }

    if (!await roleManager.RoleExistsAsync("Teacher"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Teacher", NormalizedName = "TEACHER" });
    }

    if (!await roleManager.RoleExistsAsync("Student"))
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Student", NormalizedName = "STUDENT" });
    }

    // Seed user if not already present
    if (!await userManager.Users.AnyAsync())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "ntt112004h@gmail.com",
            Email = "ntt112004h@gmail.com",
            FullName = "System Administrator",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true
        };

        var userResult = await userManager.CreateAsync(adminUser, "Thinh123!");

        if (userResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
        }
    }

    await context.SaveChangesAsync();  
}

