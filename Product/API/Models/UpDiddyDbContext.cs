using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UpDiddyApi.Models
{


    public class UpDiddyDbContextFactory : IDesignTimeDbContextFactory<UpDiddyDbContext>
    {
       
        public UpDiddyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UpDiddyDbContext>();
            var CurrentDir = System.IO.Directory.GetCurrentDirectory();
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                   .SetBasePath(CurrentDir)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = configBuilder.Build();   

            var VaultUrl = config["Vault"];
            var VaultClientId = config["Clientid"];
            var VaultSecret = config["ClientSecret"];

            configBuilder.AddAzureKeyVault(
                 VaultUrl,
                 VaultClientId,
                 VaultSecret);

            config = configBuilder.Build();
            // Get the connection string from the Azure secret vault
            var SqlConnectionString = config["CareerCircleSqlConnection"];
            optionsBuilder.UseSqlServer(SqlConnectionString);
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
        public DbSet<Course> Course { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<BadgeCourse> BadgeCourse { get; set; }
        public DbSet<BadgeEarned> BadgeEarned { get; set; }
        public DbSet<BadgeSet> BadgeSet { get; set; }
        public DbSet<EducationLevel> EducationLevel { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<VendorTermsOfService> VendorTermsOfService { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<NewsType> NewsType { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<CourseDelivery> CourseDelivery { get; set; }
        public DbSet<TagCourse> TagCourse { get; set; }
        public DbSet<TagTopic> TagTopic { get; set; }
        public DbSet<CourseReview> CourseReview { get; set; }
        public DbSet<CommunicationType> CommunicationType { get; set; }
        public DbSet<CommunicationTemplate> CommunicationTemplate { get; set; }
        public DbSet<CommunicationSubscription> CommunicationSubscription { get; set; }
        public DbSet<ReportEnrollmentByVendor> ReportEnrollmentByVendors { get; set; }
        public DbSet<Rebate> Rebate { get; set; }
        public DbSet<VendorPromoCode> VendorPromoCode { get; set; }
        public DbSet<PromoCodes> PromoCode { get; set; }
        public DbSet<SubscriberPromoCode> SubscriberPromoCode { get; set; }
        public DbSet<CoursePromoCode> CoursePromoCode { get; set; }
        public DbSet<PromoType> PromoType { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentBatch> PaymentBatch { get; set; }
        public DbSet<PaymentStatus> PaymentStatus { get; set; }
        public DbSet<PaymentProcessor> PaymentProcessor { get; set; }
        public DbSet<EnrollmentStatus> EnrollmentStatus { get; set; }
        public DbSet<VendorStudentLogin> VendorStudentLogin { get; set; }
        public DbSet<WozTransactionLog> WozTransactionLog { get; set; }
        public DbSet<WozCourseSection> WozCourseSection { get; set; }
        public DbSet<WozCourseEnrollment> WozCourseEnrollment { get; set; }

        public DbSet<WozTermsOfService> WozTermsOfService { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

         
        }
    }


}

