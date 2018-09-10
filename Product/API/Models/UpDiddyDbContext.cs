using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UpDiddyApi.Models
{


    public class UpDiddyDbContextFactory : IDesignTimeDbContextFactory<UpDiddyDbContext>
    {
        public UpDiddyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UpDiddyDbContext>();
            //TODO Make Secret
            optionsBuilder.UseSqlServer("Server=tcp:careercircle.database.windows.net,1433;Initial Catalog=careercircledb;Persist Security Info=False;User ID=CareerCircleSa;Password=T4454dSDUcKqc@dt1qu9jp&YA#o#iu!#pY@!LF&535252;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new UpDiddyDbContext(optionsBuilder.Options);
        }
    }

    public class UpDiddyDbContext : DbContext
    {
        public UpDiddyDbContext(DbContextOptions<UpDiddyDbContext> options) : base(options) { }

        public DbSet<Topic> Topic { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<Subscriber> Subscriber { get; set; }
        public DbSet<Enrollment> Enrollment { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
       //     modelBuilder.Entity<Topic>().ToTable("Topic");         
        }
    }


}

