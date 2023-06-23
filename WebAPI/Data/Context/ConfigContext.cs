using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Database;

namespace WebAPI.Data.Context
{
    public class ConfigContext : DbContext
    {
        public ConfigContext(DbContextOptions<ConfigContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<DataFactory> DataFactories { get; set; }
        public DbSet<Database.LinkedService> LinkedServices { get; set; }
        public DbSet<Database.Dataset> Datasets { get; set; }
        public DbSet<Database.SalesforceConfig> SalesforceConfigs { get; set; }
        public DbSet<Database.AzureSqlConfig> AzureSqlConfigs { get; set; }
        public DbSet<Database.Pipeline> Pipelines { get; set; }
        public DbSet<Database.CopyActivity> CopyActivities { get; set; }
        public DbSet<Database.CopyActivitySink> CopyActivitySources { get; set; }
        public DbSet<Database.CopyActivitySource> CopyActivitySinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataFactory>().ToTable("DataFactory", "Config");
            modelBuilder.Entity<Database.LinkedService>().ToTable("LinkedService", "Config");
            modelBuilder.Entity<Database.LinkedService>()
                .Property(c => c.ServiceType)
                .HasConversion<int>();
            modelBuilder.Entity<Database.Dataset>().ToTable("Dataset", "Config");
            modelBuilder.Entity<Database.Dataset>()
                .Property(c => c.ServiceType)
                .HasConversion<int>();
            modelBuilder.Entity<Database.Pipeline>().ToTable("Pipeline", "Config");
            modelBuilder.Entity<Database.SalesforceConfig>().ToTable("SalesforceConfig", "Config");
            modelBuilder.Entity<Database.AzureSqlConfig>().ToTable("AzureSqlConfig", "Config");
            modelBuilder.Entity<Database.CopyActivity>().ToTable("CopyActivity", "Config");
            modelBuilder.Entity<Database.CopyActivitySink>().ToTable("CopyActivitySink", "Config");
            modelBuilder.Entity<Database.CopyActivitySource>().ToTable("CopyActivitySource", "Config");
        }
    }
}
