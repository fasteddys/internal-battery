using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using UpDiddyLib.Shared;

namespace UpDiddyApi.Models
{


    public class UpDiddyDbContextFactory : IDesignTimeDbContextFactory<UpDiddyDbContext>
    {
        // do not remove this; there must be a parameterless constructor for ef migrations to function
        public UpDiddyDbContextFactory() { }

        public UpDiddyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UpDiddyDbContext>();
            
            var CurrentDir = System.IO.Directory.GetCurrentDirectory();
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            string Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string SettingsFile = $"appsettings.{Env}.json";
            bool IsEnvLocal = Env == 'Development';
            IConfiguration config;
            // if development file exists then this is being executed locally
            if(IsEnvLocal)
            {
                configBuilder
                    .SetBasePath(CurrentDir)
                    .AddJsonFile(SettingsFile, optional: false, reloadOnChange: true)
                    .AddUserSecrets<Startup>();

                config = configBuilder.Build();
            } else
            {
                // else it is being executed in the cloud and retrieve environmental variables (for now)
                // todo: utilize certificate instead of direct access to vault password
                // todo: task to devops the ability to generate scripts as an artifact
                configBuilder.AddEnvironmentVariables();
                config = configBuilder.Build();
                
                configBuilder.AddAzureKeyVault(config["Vault:Url"], 
                    config["Vault:ClientId"],
                    config["Vault:ClientSecret"], 
                    new KeyVaultSecretManager());
            }

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
        public DbSet<PromoCode> PromoCode { get; set; }
        public DbSet<SubscriberPromoCode> SubscriberPromoCode { get; set; }
        public DbSet<CourseVariantPromoCode> CourseVariantPromoCode { get; set; }
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
        public DbSet<RedemptionStatus> RedemptionStatus { get; set; }
        public DbSet<PromoCodeRedemption> PromoCodeRedemption { get; set; }
        public DbSet<EnrollmentLog> EnrollmentLog { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<CourseVariant> CourseVariant { get; set; }
        public DbSet<CourseVariantType> CourseVariantType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PromoCode>()
                .Property(pc => pc.NumberOfRedemptions)
                .HasDefaultValue(0);
            modelBuilder.Entity<PromoCode>()
                .Property(pc => pc.MaxAllowedNumberOfRedemptions)
                .HasDefaultValue(1);
            // this caused a problem when cleaning up abandoned promo codes. may still want a constraint in the future, but needs to be implemented differently
            //modelBuilder.Entity<PromoCodeRedemption>()
            //    .HasIndex(i => new { i.PromoCodeId, i.SubscriberId, i.CourseId, i.RedemptionStatusId, i.IsDeleted }).IsUnique();

            modelBuilder.Entity<RedemptionStatus>().HasData(
                new RedemptionStatus()
                {
                    CreateDate = DateTime.MinValue,
                    CreateGuid = Guid.Parse("D5533B5C-6C87-4C48-B9BE-D6FFB5532A4C"), // todo: this should refer to an admin account's AD Guid
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    Name = "In Process",
                    RedemptionStatusGuid = Guid.Parse("1FE97CDE-3A2D-42F1-8B8D-42824367020B"),
                    RedemptionStatusId = 1
                },
                new RedemptionStatus()
                {
                    CreateDate = DateTime.MinValue,
                    CreateGuid = Guid.Parse("D5533B5C-6C87-4C48-B9BE-D6FFB5532A4C"), // todo: this should refer to an admin account's AD Guid
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    Name = "Completed",
                    RedemptionStatusGuid = Guid.Parse("1FE97CDE-3A2D-42F1-8B8D-42824367020B"),
                    RedemptionStatusId = 2
                });
            modelBuilder.Entity<PromoType>().HasData(
                new PromoType()
                {
                    CreateDate = DateTime.MinValue,
                    CreateGuid = Guid.Parse("D5533B5C-6C87-4C48-B9BE-D6FFB5532A4C"), // todo: this should refer to an admin account's AD Guid
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    Description = "This type indicates that the PromoValueFactor is the dollar amount that should be subtracted from the course cost.",
                    Name = "Dollar Amount",
                    PromoTypeGuid = Guid.Parse("1DDB91F6-A6E5-4C01-A020-1DEA0AB77E95"),
                    PromoTypeId = 1
                },
                new PromoType()
                {
                    CreateDate = DateTime.MinValue,
                    CreateGuid = Guid.Parse("D5533B5C-6C87-4C48-B9BE-D6FFB5532A4C"), // todo: this should refer to an admin account's AD Guid
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    Description = "This type indicates that the the course cost should be reduced by the percentage value of the PromoValueFactor.",
                    Name = "Percent Off",
                    PromoTypeGuid = Guid.Parse("1DDB91F6-A6E5-4C01-A020-1DEA0AB77E95"),
                    PromoTypeId = 2
                });

            /* does this get called before or after migrationBuilder during "Update-Database" operation?
             * should this be done here or in the migration? currently keeping it in the migration since the order of operations is important 
             * (e.g. first create a column, then populate it with data from the related table, then create the FK, then drop the unused column(s))
             
            modelBuilder.Entity<CourseVariantType>().HasData(
                new CourseVariantType()
                {
                    CourseVariantTypeId = 1,
                    CourseVariantGuid = Guid.Parse("97EFDC73-8295-4C6B-B68A-07F29DE55808"),
                    CreateDate = DateTime.MinValue,
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    CreateGuid = Guid.Parse("EEA8EC63-0181-4566-A50C-A56271249230"),
                    Name = "SelfPaced"
                },
                new CourseVariantType()
                {
                    CourseVariantTypeId = 2,
                    CourseVariantGuid = Guid.Parse("D4C9E3A1-E24B-4003-8A02-65775056ACF0"),
                    CreateDate = DateTime.MinValue,
                    IsDeleted = 0,
                    ModifyDate = null,
                    ModifyGuid = null,
                    CreateGuid = Guid.Parse("6938B064-D1A1-4DE0-B259-53BCAE9F0C9A"),
                    Name = "InstructorLed"
                }); 
             */

            // todo: would like to be able to add check constraints here, but it seems it is only possible by editing the migration directly: https://stackoverflow.com/questions/34245449/is-it-possible-to-add-check-constraint-with-fluent-api-in-ef7
        }
    }
}

