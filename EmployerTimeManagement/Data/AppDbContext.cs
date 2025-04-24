using EmployerTimeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployerTimeManagement.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        public DbSet<WorkingStatusChangeEntry> WorkingStatusChanges { get; set; }
        public DbSet<HolidayEntry> Holidays { get; set; }
        public DbSet<E1Entry> E1Entries { get; set; }
        public DbSet<E2Entry> E2Entries { get; set; }
        public DbSet<E3Entry> E3Entries { get; set; }
        public DbSet<E4Entry> E4Entries { get; set; }
        public DbSet<E5Entry> E5Entries { get; set; }
        public DbSet<E6Entry> E6Entries { get; set; }
        public DbSet<E7Entry> E7Entries { get; set; }
        public DbSet<E9Entry> E9Entries { get; set; }
        public DbSet<E10Entry> E10Entries { get; set; }








        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=employer.db");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Set required constraints or default values
            modelBuilder.Entity<Employee>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<CompanyInfo>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<AppUser>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.Employee)
                .WithMany(e => e.WorkLogs)
                .HasForeignKey(w => w.EmployeeId);
        }
    }
}
