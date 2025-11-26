using Korlavalasa.Data;
using Korlavalasa.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
    options.ValueLengthLimit = 10 * 1024 * 1024; // 10MB per file
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});

// DATABASE CONFIGURATION - UPDATED FOR PRODUCTION
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    // Local Development - SQL Server
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Production - PostgreSQL (Render)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Identity Configuration
builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;

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

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();


// Database Initialization and Seeding - NUCLEAR OPTION
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AdminUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        Console.WriteLine("🚀 STARTING DATABASE INITIALIZATION...");

        // Nuclear option: Delete and recreate database
        Console.WriteLine("🔧 Dropping existing database...");
        await context.Database.EnsureDeletedAsync();
        Console.WriteLine("✅ Database dropped");

        Console.WriteLine("🔧 Creating new database tables...");
        var created = await context.Database.EnsureCreatedAsync();
        Console.WriteLine($"✅ Database tables created: {created}");

        if (created)
        {
            Console.WriteLine("🔧 Seeding initial data...");
            await SeedInitialData(context, userManager, roleManager);
            Console.WriteLine("✅ Data seeded successfully");
        }
        else
        {
            Console.WriteLine("❌ Database tables were not created");
        }

        Console.WriteLine("🎉 DATABASE INITIALIZATION COMPLETED SUCCESSFULLY");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"💥 CRITICAL DATABASE ERROR: {ex.Message}");
        Console.WriteLine($"💥 STACK TRACE: {ex.StackTrace}");

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "A critical error occurred while initializing the database.");

        // Don't crash the app, but log everything
        if (ex.InnerException != null)
        {
            Console.WriteLine($"💥 INNER EXCEPTION: {ex.InnerException.Message}");
        }
    }
}

app.Run();

// Seed Initial Data Method
async Task SeedInitialData(AppDbContext context, UserManager<AdminUser> userManager, RoleManager<IdentityRole> roleManager)
{
    try
    {
        Console.WriteLine("🔧 Starting data seeding...");

        // Create Admin Role if it doesn't exist
        Console.WriteLine("🔧 Creating admin role...");
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (roleResult.Succeeded)
            {
                Console.WriteLine("✅ Admin role created");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine("✅ Admin role already exists");
        }

        // Create Admin User if it doesn't exist
        Console.WriteLine("🔧 Creating admin user...");
        string adminEmail = "admin@korlavalasa.com";
        string adminUsername = "admin";
        string adminPassword = "Admin@123";
        string adminFullName = "Korlavalasa Administrator";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AdminUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (userResult.Succeeded)
            {
                Console.WriteLine("✅ Admin user created successfully");

                // Add to admin role
                var roleAddResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleAddResult.Succeeded)
                {
                    Console.WriteLine("✅ Admin user added to Admin role");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to add admin user to role: {string.Join(", ", roleAddResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                Console.WriteLine($"❌ Failed to create admin user: {errors}");
            }
        }
        else
        {
            Console.WriteLine("✅ Admin user already exists");
        }

        // Seed Village Info if it doesn't exist
        Console.WriteLine("🔧 Seeding village information...");
        if (!context.VillageInfo.Any())
        {
            try
            {
                var villageInfo = new VillageInfo
                {
                    AboutText = "Welcome to Korlavalasa, a beautiful village in Andhra Pradesh known for its rich cultural heritage and ancient temples. Our village represents the perfect blend of traditional values and modern development.",
                    History = "Korlavalasa has a glorious history spanning over two centuries. Established by our ancestors who were primarily involved in agriculture, the village has preserved ancient traditions while progressing with time.",
                    Population = 3250,
                    Area = 18.5m,
                    MainCrops = "Paddy, Sugarcane, Cotton, Groundnut, Vegetables",
                    ContactEmail = "contact@korlavalasa.com",
                    ContactNumber = "+91-9876543210",
                    Address = "Korlavalasa Village, Vizianagaram District, Andhra Pradesh - 535002, India",
                    SarpanchName = "Sri Venkata Ramana",
                    SecretaryName = "Sri Srinivasa Rao"
                };

                context.VillageInfo.Add(villageInfo);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Village information seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding village info: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("✅ Village information already exists");
        }

        // Seed Sample News if none exist
        Console.WriteLine("🔧 Seeding news...");
        if (!context.News.Any())
        {
            try
            {
                var newsItems = new List<News>
                {
                    new News
                    {
                        Title = "Welcome to Korlavalasa Official Website",
                        Content = "We are excited to launch the official website of Korlavalasa village. This digital platform will help us stay connected and share important updates with all villagers.",
                        PublishedDate = DateTime.Now.AddDays(-1),
                        IsActive = true
                    },
                    new News
                    {
                        Title = "Annual Village Festival",
                        Content = "The annual village festival will be celebrated next month. All villagers are invited to participate in the cultural programs and traditional rituals.",
                        PublishedDate = DateTime.Now.AddDays(-3),
                        IsActive = true
                    }
                };

                context.News.AddRange(newsItems);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Sample news items seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding news: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("✅ News already exists");
        }

        // Seed Sample Events if none exist
        Console.WriteLine("🔧 Seeding events...");
        if (!context.Events.Any())
        {
            try
            {
                var events = new List<Event>
                {
                    new Event
                    {
                        Title = "Monthly Gram Sabha Meeting",
                        Description = "Regular village council meeting to discuss development works and community issues.",
                        EventDate = DateTime.Now.AddDays(7),
                        Location = "Village Panchayat Office",
                        ContactPerson = "Sarpanch Office",
                        ContactNumber = "+91-9876543210"
                    },
                    new Event
                    {
                        Title = "Village Temple Festival",
                        Description = "Annual celebration at the village temple with cultural programs and traditional rituals.",
                        EventDate = DateTime.Now.AddDays(15),
                        Location = "Sri Lakshmi Narasimha Temple",
                        ContactPerson = "Temple Committee",
                        ContactNumber = "+91-9876543211"
                    }
                };

                context.Events.AddRange(events);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Sample events seeded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding events: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("✅ Events already exist");
        }

        Console.WriteLine("🎉 Data seeding completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"💥 CRITICAL ERROR in data seeding: {ex.Message}");
        Console.WriteLine($"💥 STACK TRACE: {ex.StackTrace}");
        throw; // Re-throw to see the exact error in logs
    }

   
}