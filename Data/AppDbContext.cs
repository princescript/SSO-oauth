using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserMaster> DbUserMaster { get; set; }
        public DbSet<AuthSession> DbAuthSession { get; set; }
        public DbSet<ProductMaster> DbProduct { get; set; }
        public DbSet<TranUserProductAccess> DbUserProductAccess { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserMaster>(entity =>
            {
                entity.ToTable("mst_user");
                entity.HasKey(e => e.login_id);
                entity.Property(e => e.login_id)
                      .HasColumnName("login_id")
                      .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<AuthSession>(entity =>
            {
                entity.ToTable("auth_session");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id)
                      .HasColumnName("id")
                      .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<ProductMaster>(entity =>
            {
                entity.ToTable("mst_product");
                entity.HasKey(e => e.product_id);
                entity.Property(e => e.product_id)
                      .HasColumnName("product_id")
                      .ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<TranUserProductAccess>(entity =>
            {
                entity.ToTable("tran_user_product_access ");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id)
                      .HasColumnName("id")
                      .ValueGeneratedOnAdd();
            });

        }

    }
}
