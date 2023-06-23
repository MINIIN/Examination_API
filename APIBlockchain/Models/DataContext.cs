using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WebApplication3.Models.Data;

namespace WebApplication3.Models
{
    public class DataContext: DbContext
    {
        //        remove-migration
        //add-migration ScoreDB
        //enable-migrations

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DataContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer("Host=localhost;Port=5432;Database=Score;Username=postgres;Password=postgres"); TrustServerCertificate=True;
            optionsBuilder.UseSqlServer(@"Server=.\SQLExpress;Database=Score;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Subject>()
        //        .HasNoKey();
        //}

        public DbSet<User> User { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<Score_user> Score_user { get; set; }
        public DbSet<Register_subject> Register_subject { get; set; }
    }
}
