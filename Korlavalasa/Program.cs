using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// FIXED: Increase file upload limits for multiple files
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB for multiple files
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB for multiple files
});

// ADD THIS: Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.ValueLengthLimit = 10 * 1024 * 1024; // 10MB per file
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// Add DbContext with Identity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services - RELAXED PASSWORD REQUIREMENTS
builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
{
    // ✅ RELAXED Password settings
    options.Password.RequireDigit = true;         
    options.Password.RequireLowercase = true;       // Still require lowercase
    options.Password.RequireNonAlphanumeric = false; // No special characters
    options.Password.RequireUppercase = false;      // No uppercase
    options.Password.RequiredLength = 4;            // Minimum 4 characters
    options.Password.RequiredUniqueChars = 1;       // At least 1 unique character

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // Show detailed errors in development
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add Authentication & Authorization - MUST be in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Initialize database and create admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AdminUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // This will create the database and apply migrations
        context.Database.Migrate();

        // Seed admin user and roles
        await SeedAdminUser(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

async Task SeedAdminUser(UserManager<AdminUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create Admin role
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // ✅ FIXED CREDENTIALS - Now compatible with relaxed requirements
    string adminEmail = "admin@123.com";
    string adminUsername = "admin";
    string adminPassword = "kvadmin@145";  
    string adminFullName = "Korlavalasa Admin";

    // Check if admin user exists
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        // Create new admin user
        adminUser = new AdminUser
        {
            UserName = adminUsername,
            Email = adminEmail,
            FullName = adminFullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            // Log success
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("✅ Admin user created successfully with username: {Username}", adminUsername);
        }
        else
        {
            // Log the specific errors
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError("❌ Failed to create admin user: {Errors}", errors);

            // Don't throw exception, just log it
            Console.WriteLine($"Failed to create admin user: {errors}");
        }
    }
    else
    {
        // Admin user already exists - reset password
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        var result = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);

        if (result.Succeeded)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("✅ Admin user password reset successfully for: {Username}", adminUsername);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError("❌ Failed to reset admin password: {Errors}", errors);
        }
    }
}