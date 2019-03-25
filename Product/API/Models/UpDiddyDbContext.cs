﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using UpDiddyLib.Shared;
using UpDiddyApi.Models.Views;

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
            if (string.IsNullOrEmpty(Env))
                Env = "Development";
            string SettingsFile = $"appsettings.{Env}.json";
            bool IsEnvLocal = Env == "Development";
            IConfiguration config;
            // if development file exists then this is being executed locally
            if (IsEnvLocal)
            {
                configBuilder
                    .SetBasePath(CurrentDir)
                    .AddJsonFile(SettingsFile, optional: false, reloadOnChange: true)
                    .AddUserSecrets<Startup>();

                config = configBuilder.Build();
            }
            else
            {
                // else it is being executed in the cloud and retrieve environmental variables (for now)
                configBuilder.AddEnvironmentVariables();
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
        public DbSet<Refund> Refund { get; set; }
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
        public DbSet<LinkedInToken> LinkedInToken { get; set; }
        public DbSet<SubscriberProfileStagingStore> SubscriberProfileStagingStore { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<SubscriberSkill> SubscriberSkill { get; set; }
        public DbSet<SubscriberFile> SubscriberFile { get; set; }
        public DbSet<SubscriberWorkHistory> SubscriberWorkHistory { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CompensationType> CompensationType { get; set; }
        public DbSet<SubscriberEducationHistory> SubscriberEducationHistory { get; set; }
        public DbSet<EducationalInstitution> EducationalInstitution { get; set; }
        public DbSet<EducationalDegreeType> EducationalDegreeType { get; set; }
        public DbSet<EducationalDegree> EducationalDegree { get; set; }
        public DbSet<CourseSkill> CourseSkill { get; set; }
        public DbSet<Campaign> Campaign { get; set; }
        public DbSet<Action> Action { get; set; }
        public DbSet<ContactAction> ContactAction { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<CampaignCourseVariant> CampaignCourseVariant { get; set; }
        public DbSet<RebateType> RebateType { get; set; }
        public DbSet<CampaignContact> CampaignContact { get; set; }
        public DbSet<CampaignPhase> CampaignPhase { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<Partner> Partner { get; set; }
        public DbSet<PartnerContact> PartnerContact { get; set; }
        public DbSet<PartnerReferrer> PartnerReferrer { get; set; }
        public DbSet<SubscriberAction> SubscriberAction { get; set; }
        public DbSet<EntityType> EntityType { get; set; }

        #region DBQueries

        public DbQuery<CampaignStatistic> CampaignStatistic { get; set; }
        public DbQuery<CampaignDetail> CampaignDetail { get; set; }
        public DbQuery<v_SubscriberSources> SubscriberSources {get; set; }
        public DbQuery<v_SubscriberSignUpPartnerReference> SubscriberSignUpPartnerReferences { get; set; }
        public DbQuery<SubscriberSearch> SubscriberSearch { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubscriberAction>()
                .HasKey(sa => new { sa.SubscriberId, sa.ActionId });

            modelBuilder.Entity<EntityType>()
                .HasIndex(et => et.Name)
                .IsUnique();

            modelBuilder.Entity<SubscriberAction>()
                .Property(sa => sa.OccurredDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .Query<v_SubscriberSources>()
                .ToView("v_SubscriberSources");

            modelBuilder
                .Query<v_SubscriberSignUpPartnerReference>()
                .ToView("v_SubscriberSignUpPartnerReferences");

            modelBuilder.Entity<Campaign>()
                .HasIndex(pc => pc.Name)
                .IsUnique();

            modelBuilder.Entity<PartnerContact>()
                .HasKey(pc => new { pc.PartnerId, pc.ContactId });

            modelBuilder.Entity<Contact>()
                .HasMany<PartnerContact>(c => c.PartnerContacts);

            modelBuilder.Entity<PartnerContact>()
                .Property<string>("MetaDataJSON")
                .HasField("_metadata");

            modelBuilder.Entity<Partner>()
                .HasMany<PartnerReferrer>(e => e.Referrers);

            modelBuilder.Entity<Enrollment>()
                .HasOne<CampaignCourseVariant>(e => e.CampaignCourseVariant)
                .WithMany();

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<CampaignContact>()
                .HasKey(cc => new { cc.CampaignId, cc.ContactId });

            modelBuilder.Entity<CampaignCourseVariant>()
                .HasKey(ccv => new { ccv.CampaignId, ccv.CourseVariantId });

            modelBuilder.Entity<ContactAction>()
                .HasKey(ca => new { ca.ContactId, ca.CampaignId, ca.ActionId, ca.CampaignPhaseId });

            modelBuilder.Entity<ContactAction>()
                .Property(ca => ca.OccurredDate)
                .HasDefaultValueSql("GETUTCDATE()"); // must use sql function instead of c# function so that the current date is used on insert (rather than the initialized value from the migration)

            modelBuilder.Entity<RebateType>()
                .HasIndex(rt => rt.Name)
                .IsUnique();

            modelBuilder.Entity<CourseVariantPromoCode>()
                .Property(spc => spc.NumberOfRedemptions)
                .HasDefaultValue(0);

            modelBuilder.Entity<SubscriberPromoCode>()
                .Property(spc => spc.NumberOfRedemptions)
                .HasDefaultValue(0);

            modelBuilder.Entity<VendorPromoCode>()
                .Property(spc => spc.NumberOfRedemptions)
                .HasDefaultValue(0);

            modelBuilder.Entity<Skill>()
                .HasIndex(u => u.SkillName)
                .IsUnique();

            modelBuilder.Entity<SubscriberSkill>()
                .HasKey(ss => new { ss.SkillId, ss.SubscriberId });

            modelBuilder.Entity<CourseSkill>()
                .HasKey(cs => new { cs.CourseId, cs.SkillId });

            modelBuilder.Entity<PromoCode>()
                .Property(pc => pc.NumberOfRedemptions)
                .HasDefaultValue(0);

            modelBuilder.Entity<PromoCode>()
                .Property(pc => pc.MaxAllowedNumberOfRedemptions)
                .HasDefaultValue(1);
        }
    }
}

