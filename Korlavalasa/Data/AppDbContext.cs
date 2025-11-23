using Korlavalasa.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Korlavalasa.Data
{
    public class AppDbContext : IdentityDbContext<AdminUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<News> News { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<GalleryImage> Gallery { get; set; }
        public DbSet<VillageInfo> VillageInfo { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure VillageInfo
            builder.Entity<VillageInfo>(entity =>
            {
                entity.Property(v => v.Area)
                      .HasPrecision(18, 2); // 18 total digits, 2 decimal places

                entity.Property(v => v.AboutText)
                      .HasMaxLength(2000);

                entity.Property(v => v.History)
                      .HasMaxLength(3000);

                entity.Property(v => v.MainCrops)
                      .HasMaxLength(500);

                entity.Property(v => v.Address)
                      .HasMaxLength(500);
            });

            // Configure News
            builder.Entity<News>(entity =>
            {
                entity.Property(n => n.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(n => n.Content)
                      .HasMaxLength(5000)
                      .IsRequired();

                entity.Property(n => n.ImageUrl)
                      .HasMaxLength(500);

                entity.HasIndex(n => n.PublishedDate);
                entity.HasIndex(n => n.IsActive);
            });

            // Configure Event
            builder.Entity<Event>(entity =>
            {
                entity.Property(e => e.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(1000);

                entity.Property(e => e.Location)
                      .HasMaxLength(200);

                entity.Property(e => e.ContactPerson)
                      .HasMaxLength(100);

                entity.HasIndex(e => e.EventDate);
            });

            // Configure GalleryImage
            builder.Entity<GalleryImage>(entity =>
            {
                entity.Property(g => g.Title)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(g => g.ImagePath)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(g => g.Category)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(g => g.Description)
                      .HasMaxLength(500);

                entity.HasIndex(g => g.Category);
                entity.HasIndex(g => g.UploadDate);
            });

            //// Seed initial VillageInfo data
            //builder.Entity<VillageInfo>().HasData(
            //    new VillageInfo
            //    {
            //        Id = 1,
            //        AboutText = "Welcome to Korlavalasa village official website.",
            //        History = "Korlavalasa has a rich history...",
            //        Population = 0, // You can set initial values
            //        Area = 0,
            //        MainCrops = "Rice, Vegetables, Fruits",
            //        ContactEmail = "info@korlavalasa.com",
            //        ContactNumber = "+91XXXXXXXXXX",
            //        Address = "Korlavalasa Village",
            //        SarpanchName = "Sarpanch Name",
            //        SecretaryName = "Secretary Name"
            //    }
            //);
        }
    }
}