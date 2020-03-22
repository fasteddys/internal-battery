using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Domain.Models;
using System.Collections.Generic;
using UpDiddyLib.Domain.Models.Reports;
using UpDiddyApi.Models.G2;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models.G2;

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

        #region DBEntities

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
        public DbSet<Contact> Contact { get; set; }
        public DbSet<CampaignCourseVariant> CampaignCourseVariant { get; set; }
        public DbSet<RebateType> RebateType { get; set; }
        public DbSet<CampaignPhase> CampaignPhase { get; set; }
        public DbSet<JobPosting> JobPosting { get; set; }
        public DbSet<EmploymentType> EmploymentType { get; set; }
        public DbSet<SecurityClearance> SecurityClearance { get; set; }
        public DbSet<Industry> Industry { get; set; }
        public DbSet<ExperienceLevel> ExperienceLevel { get; set; }
        public DbSet<JobPostingSkill> JobPostingSkill { get; set; }
        public DbSet<JobCategory> JobCategory { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<Partner> Partner { get; set; }
        public DbSet<PartnerContact> PartnerContact { get; set; }
        public DbSet<PartnerReferrer> PartnerReferrer { get; set; }
        public DbSet<PartnerWebRedirect> PartnerWebRedirect { get; set; }
        public DbSet<SubscriberAction> SubscriberAction { get; set; }
        public DbSet<EntityType> EntityType { get; set; }
        public DbSet<PartnerType> PartnerType { get; set; }
        public DbSet<LeadStatus> LeadStatus { get; set; }
        public DbSet<PartnerContactLeadStatus> PartnerContactLeadStatus { get; set; }
        public DbSet<PartnerContactFile> PartnerContactFile { get; set; }
        public DbSet<PartnerContactFileLeadStatus> PartnerContactFileLeadStatus { get; set; }
        public DbSet<CampaignPartnerContact> CampaignPartnerContact { get; set; }
        public DbSet<PartnerContactAction> PartnerContactAction { get; set; }
        public DbSet<JobApplication> JobApplication { get; set; }
        public DbSet<RecruiterCompany> RecruiterCompany { get; set; }
        public DbSet<JobPostingFavorite> JobPostingFavorite { get; set; }
        public DbSet<Recruiter> Recruiter { get; set; }
        public DbSet<RecruiterAction> RecruiterAction { get; set; }
        public DbSet<JobSite> JobSite { get; set; }
        public DbSet<JobPage> JobPage { get; set; }
        public DbSet<JobPageStatus> JobPageStatus { get; set; }
        public DbSet<JobSiteScrapeStatistic> JobSiteScrapeStatistic { get; set; }
        public DbSet<ZeroBounce> ZeroBounce { get; set; }
        public DbSet<JobPostingAlert> JobPostingAlert { get; set; }
        public DbSet<JobReferral> JobReferral { get; set; }
        public DbSet<SubscriberNotes> SubscriberNotes { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<SubscriberNotification> SubscriberNotification { get; set; }
        public DbSet<ResumeParse> ResumeParse { get; set; }
        public DbSet<ResumeParseResult> ResumeParseResult { get; set; }
        public DbSet<CampaignPartner> CampaignPartner { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<SubscriberGroup> SubscriberGroup { get; set; }
        public DbSet<GroupPartner> GroupPartner { get; set; }
        public DbSet<SalesForceSignUpList> SalesForceSignUpList { get; set; }
        public DbSet<Traitify> Traitify { get; set; }
        public DbSet<TraitifyCourseTopicBlendMapping> TraitifyBlendCourseTopicMapping { get; set; }
        public DbSet<ServiceOffering> ServiceOffering { get; set; }
        public DbSet<ServiceOfferingItem> ServiceOfferingItem { get; set; }
        public DbSet<ServiceOfferingOrder> ServiceOfferingOrder { get; set; }
        public DbSet<ServiceOfferingPromoCodeRedemption> ServiceOfferingPromoCodeRedemption { get; set; }
        public DbSet<ServiceOfferingPromoCode> ServiceOfferingPromoCode { get; set; }
        public DbSet<FileDownloadTracker> FileDownloadTracker { get; set; }
        public DbSet<CourseSite> CourseSite { get; set; }
        public DbSet<CoursePage> CoursePage { get; set; }
        public DbSet<CourseFavorite> CourseFavorite { get; set; }
        public DbSet<CoursePageStatus> CoursePageStatus { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Postal> Postal { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequest { get; set; }
        public DbSet<TalentFavorite> TalentFavorite { get; set; }
        public DbSet<CourseLevel> CourseLevel { get; set; }
        public DbSet<CourseReferral> CourseReferral { get; set; }
        public DbSet<NotificationGroup> NotificationGroup { get; set; }
        public DbSet<SendGridEvent> SendGridEvent { get; set; }
        public DbSet<SubscriberSendGridEvent> SubscriberSendGridEvent { get; set; }
        public DbSet<HiringSolvedResumeParse> HiringSolvedResumeParse { get; set; }
        public DbSet<SovrenParseStatistic> SovrenParseStatistics { get; set; }
        public DbSet<Profile> Profile { get; set; }
        public DbSet<ContactType> ContactType { get; set; }
        public DbSet<ProfileComment> ProfileComment { get; set; }
        public DbSet<ProfileDocument> ProfileDocument { get; set; }
        public DbSet<ProfileSearchLocation> ProfileSearchLocation { get; set; }
        public DbSet<ProfileSkill> ProfileSkill { get; set; }
        public DbSet<ProfileTag> ProfileTag { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<ProfileWishlist> ProfileWishlist { get; set; }
        public DbSet<AzureIndexStatus> AzureIndexStatus { get; set; }
        public DbSet<RecruiterStat> RecruiterStat { get; set; }
        public DbSet<ProfileEmploymentType> ProfileEmploymentType { get; set; }

        #endregion

        #region DBQueries

        public DbQuery<v_ProfileAzureSearch> ProfileAzureSearch { get; set; }
        public DbQuery<CampaignStatistic> CampaignStatistic { get; set; }
        public DbQuery<CampaignDetail> CampaignDetail { get; set; }
        public DbQuery<v_SubscriberSources> SubscriberSources { get; set; }
        public DbQuery<v_SubscriberSignUpPartnerReference> SubscriberSignUpPartnerReferences { get; set; }
        public DbQuery<SubscriberSearch> SubscriberSearch { get; set; }
        public DbQuery<v_RecruiterSubscriberActions> RecruiterSubscriberActions { get; set; }
        public DbQuery<v_SubscriberOfferActions> SubscriberOfferActions { get; set; }
        public DbQuery<v_ThrottledLeadEmailDelivery> ThrottledLeadEmailDelivery { get; set; }
        public DbQuery<v_UnreadNotifications> UnreadNotifications { get; set; }
        public DbQuery<v_NotificationReadCounts> NotificationReadCounts { get; set; }
        public DbQuery<JobAbandonmentStatistics> JobAbandonmentStatistics { get; set; }
        public DbQuery<JobCountPerProvince> JobCountPerProvince { get; set; }
        public DbQuery<SubscriberSourceDto> SubscriberSourcesDetails { get; set; }
        public DbQuery<SubscriberInitialSourceDto> SubscriberInitialSource { get; set; }
        public DbQuery<CourseDetailDto> CourseDetails { get; set; }
        public DbQuery<CourseFavoriteDto> CourseFavorites { get; set; }
        public DbQuery<CourseVariantDetailDto> CourseVariants { get; set; }
        public DbQuery<RelatedJobDto> RelatedJobs { get; set; }
        public DbQuery<RelatedCourseDto> RelatedCourses { get; set; }
        public DbQuery<JobDto> SubscriberJobFavorites { get; set; }
        public DbQuery<SubscriberSignUpCourseEnrollmentStatistics> SubscriberSignUpCourseEnrollmentStatistics { get; set; }
        public DbQuery<SearchTermDto> KeywordSearchTerms { get; set; }
        public DbQuery<SearchTermDto> LocationSearchTerms { get; set; }
        public DbQuery<SubscriberNotesDto> SubscriberNoteQuery { get; set; }
        public DbQuery<SubscriberCourseDto> SubscriberCourses { get; set; }
        public DbQuery<JobSitemapDto> JobSitemap { get; set; }
        public DbQuery<UpDiddyLib.Dto.NotificationDto> LegacyNotifications { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.NotificationDto> Notifications { get; set; }
        public DbQuery<CompanyDto> Companies { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.TopicDto> Topics { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.CompensationTypeDto> CompensationTypes { get; set; }
        public DbQuery<CountryDetailDto> Countries { get; set; }
        public DbQuery<CourseLevelDto> CourseLevels { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.EducationLevelDto> EducationLevels { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto> EducationalDegreeTypes { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.EmploymentTypeDto> EmploymentTypes { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.ExperienceLevelDto> ExperienceLevels { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.IndustryDto> Industries { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.OfferDto> Offers { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.SecurityClearanceDto> SecurityClearances { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.SkillDto> Skills { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.StateDetailDto> States { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.TalentFavoriteDto> TalentFavorites { get; set; }
        public DbQuery<SubscriberNotesDto> SubscriberNotesDto { get; set; }
        public DbQuery<JobCrudDto> JobCruds { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.PartnerDto> Partners { get; set; }
        public DbQuery<GroupInfoDto> Groups { get; set; }
        public DbQuery<RecruiterInfoDto> Recruiters { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto> JobSiteScrapeStatistics { get; set; }
        public DbQuery<UsersDto> Users { get; set; }
        public DbQuery<UsersDetailDto> UsersDetail { get; set; }
        public DbQuery<PartnerUsers> PartnerUsers { get; set; } 
        public DbQuery<SubscriberEmailStatisticDto> SubscriberEmailStatistics { get; set; }
        public DbQuery<ProfileWishlistDto> ProfileWishlists { get; set; }
        public DbQuery<WishlistDto> Wishlists { get; set; }
        public DbQuery<RecruiterStatDto> RecruiterStats { get; set; }
        public DbQuery<CommentDto> Comments { get; set; }
        public DbQuery<UpDiddyLib.Domain.Models.TagDto> Tags { get; set; }
        public DbQuery<ProfileTagDto> ProfileTags { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfileEmploymentType>()
                .HasIndex(pet => new { pet.ProfileId, pet.EmploymentTypeId })
                .HasName("UIX_ProfileEmploymentType_Profile_EmploymentType")
                .IsUnique(true);

            modelBuilder.Entity<RecruiterCompany>()
                .HasIndex(rc => new { rc.CompanyId, rc.RecruiterId })
                .HasName("UIX_RecruiterCompany_Recruiter_Company")
                .IsUnique(true);

            modelBuilder.Entity<AzureIndexStatus>()
                .HasIndex(ais => ais.Name)
                .HasName("UIX_AzureIndexStatus_Name")
                .IsUnique(true);

            modelBuilder.Entity<ProfileTag>()
                .HasIndex(pt => new { pt.TagId, pt.ProfileId })
                .HasName("UIX_ProfileTag_Profile_Tag")
                .IsUnique(true);

            modelBuilder.Entity<ProfileWishlist>()
                .HasIndex(pw => new { pw.ProfileId, pw.WishlistId })
                .HasName("UIX_ProfileWishlist_Profile_Wishlist")
                .IsUnique(true);

            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.RecruiterId, w.Name })
                .HasName("UIX_Wishlist_Recruiter_Name")
                .IsUnique(true);

            modelBuilder
                .Query<v_ProfileAzureSearch>()
                .ToView("v_ProfileAzureSearch", "G2");

            modelBuilder.Entity<Profile>()
                .HasIndex(p => new { p.SubscriberId, p.CompanyId })
                .HasName("UIX_Profile_Subscriber_Company")
                .IsUnique(true);

            modelBuilder.Entity<ProfileSearchLocation>()
                .HasIndex(psl => new { psl.ProfileId, psl.CityId, psl.PostalId })
                .HasName("UIX_ProfileSearchLocation_Profile_City_Postal")
                .IsUnique(true);

            modelBuilder.Entity<ProfileSkill>()
                .HasIndex(ps => new { ps.ProfileId, ps.SkillId })
                .HasName("UIX_ProfileSkill_Profile_Skill")
                .IsUnique(true);

            modelBuilder.Entity<ContactType>()
                .HasIndex(ct => ct.Name)
                .HasName("UIX_ContactType_Name")
                .IsUnique(true);

            modelBuilder.Query<JobSitemapDto>()
                .Property(jsm => jsm.Url)
                .HasConversion(
                u => u.ToString(),
                u => new Uri(u));

            modelBuilder.Entity<Postal>()
                .HasIndex(p => new { p.CityId, p.Code })
                .HasName("UIX_Postal_City_Code")
                .IsUnique(true);

            modelBuilder.Entity<City>()
                .HasIndex(c => new { c.StateId, c.Name })
                .HasName("UIX_City_State_Name")
                .IsUnique(true);

            modelBuilder.Entity<CourseSite>()
                .HasIndex(c => c.Name)
                .HasName("UIX_CourseSite_Name")
                .IsUnique(true);

            modelBuilder.Entity<CoursePage>()
                .HasIndex(cp => new { cp.UniqueIdentifier, cp.CourseSiteId })
                .HasName("UIX_CoursePage_CourseSite_UniqueIdentifier")
                .IsUnique(true);

            modelBuilder.Entity<CourseSite>()
                .Property(cs => cs.Uri)
                .HasConversion(
                cs => cs.ToString(),
                cs => new Uri(cs));

            modelBuilder.Entity<CoursePage>()
                .Property(cp => cp.Uri)
                .HasConversion(
                cp => cp.ToString(),
                cp => new Uri(cp));

            modelBuilder.Entity<SubscriberNotification>()
                .HasQueryFilter(sn => sn.IsDeleted == 0);

            modelBuilder
                .Query<v_ThrottledLeadEmailDelivery>()
                .ToView("v_ThrottledLeadEmailDelivery");

            modelBuilder.Entity<CampaignPartner>()
                .Property(cp => cp.EmailSubAccountId)
                .HasMaxLength(100)
                .IsRequired(true);

            modelBuilder.Entity<CampaignPartner>()
                .Property(cp => cp.EmailTemplateId)
                .HasMaxLength(50)
                .IsRequired(true);

            modelBuilder.Entity<CampaignPartner>()
                .Property(cp => cp.IsUseSeedEmails)
                .HasDefaultValueSql("0");

            modelBuilder.Entity<JobPostingAlert>()
                .Property(a => a.Frequency)
                .HasMaxLength(10)
                .HasConversion(
                    f => f.ToString(),
                    f => (Frequency)Enum.Parse(typeof(Frequency), f));

            modelBuilder.Entity<JobPostingAlert>()
                .Property(a => a.ExecutionDayOfWeek)
                .HasMaxLength(10)
                .IsRequired(false)
                .HasConversion(
                    dow => dow.ToString(),
                    dow => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dow));

            modelBuilder.Entity<JobPostingAlert>()
                .Property<string>("JobQueryDtoJSON")
                .HasField("_jobQueryDto")
                .IsRequired(true);

            modelBuilder.Entity<JobPostingAlert>()
                .HasQueryFilter(a => a.IsDeleted == 0);

            modelBuilder.Entity<SubscriberEducationHistory>()
                .HasQueryFilter(a => a.IsDeleted == 0);

            modelBuilder.Entity<SubscriberWorkHistory>()
                .HasQueryFilter(a => a.IsDeleted == 0);

            modelBuilder.Entity<ZeroBounce>()
                .Property<string>("ResponseJSON")
                .HasField("_response");

            modelBuilder.Entity<JobPosting>()
                .HasIndex(jp => new { jp.IsDeleted, jp.JobPostingGuid })
                .HasName("IX_JobPosting_IsDeletedJobPostingGuid");

            modelBuilder.Entity<JobSite>()
                .HasIndex(js => js.Name)
                .HasName("UIX_JobSite_Name")
                .IsUnique(true);

            modelBuilder.Entity<JobPage>()
                .HasIndex(jp => new { jp.UniqueIdentifier, jp.JobSiteId })
                .HasName("UIX_JobPage_JobSite_UniqueIdentifier")
                .IsUnique(true);

            modelBuilder.Entity<JobSite>()
                .Property(js => js.Uri)
                .HasConversion(
                js => js.ToString(),
                js => new Uri(js));

            modelBuilder.Entity<JobPage>()
                .Property(jp => jp.Uri)
                .HasConversion(
                jp => jp.ToString(),
                jp => new Uri(jp));

            modelBuilder.Entity<RecruiterAction>()
                .Property(ra => ra.OccurredDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<PartnerContactFileLeadStatus>()
                .HasKey(pcfls => new { pcfls.PartnerContactFileId, pcfls.LeadStatusId });

            modelBuilder.Entity<PartnerContactFile>()
                .Property<string>(pcf => pcf.Base64EncodedData)
                .IsUnicode(false);

            modelBuilder.Entity<PartnerContactLeadStatus>()
                .HasKey(pcls => new { pcls.PartnerContactId, pcls.LeadStatusId });

            modelBuilder.Entity<LeadStatus>()
                .Property(ls => ls.Severity)
                .HasConversion(
                ls => ls.ToString(),
                ls => (Severity)Enum.Parse(typeof(Severity), ls));

            modelBuilder.Entity<PartnerType>()
                .HasIndex(pt => pt.Name)
                .IsUnique();

            modelBuilder
                .Query<v_SubscriberOfferActions>()
                .ToView("v_SubscriberOfferActions");

            modelBuilder
                .Query<v_RecruiterSubscriberActions>()
                .ToView("v_RecruiterSubscriberActions");

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

            modelBuilder.Entity<Campaign>()
                .Property(e => e.TargetedViewName)
                .HasColumnType("varchar(100)");

            modelBuilder.Entity<Contact>()
                .HasMany<PartnerContact>(c => c.PartnerContacts);

            modelBuilder.Entity<PartnerContact>()
                .Property<string>("MetaDataJSON")
                .HasField("_metadata");

            modelBuilder.Entity<PartnerContact>()
                .HasIndex(pc => pc.PartnerContactGuid)
                .HasName("UIX_PartnerContact_PartnerContactGuid")
                .IsUnique(true);

            modelBuilder.Entity<PartnerContactFile>()
                .HasOne(e => e.PartnerContact)
                .WithMany(e => e.PartnerContactFiles);

            modelBuilder.Entity<Partner>()
                .HasMany<PartnerReferrer>(e => e.Referrers);

            modelBuilder.Entity<Partner>()
                .HasOne<PartnerWebRedirect>(e => e.WebRedirect);

            modelBuilder.Entity<Enrollment>()
                .HasOne<CampaignCourseVariant>(e => e.CampaignCourseVariant)
                .WithMany();

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<CampaignPartnerContact>()
                .HasIndex(e => e.TinyId)
                .HasName("UIX_CampaignPartnerContact_TinyId")
                .IsUnique(true);

            modelBuilder.Entity<CampaignPartnerContact>()
                .Property(e => e.TinyId)
                .HasColumnType("char(8)");

            modelBuilder.Entity<CampaignPartnerContact>()
                .HasKey(cpc => new { cpc.CampaignId, cpc.PartnerContactId });

            modelBuilder.Entity<CampaignCourseVariant>()
                .HasKey(ccv => new { ccv.CampaignId, ccv.CourseVariantId });

            modelBuilder.Entity<PartnerContactAction>()
                .HasKey(pca => new { pca.PartnerContactId, pca.CampaignId, pca.ActionId, pca.CampaignPhaseId });

            modelBuilder.Entity<PartnerContactAction>()
                .Property(pca => pca.OccurredDate)
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

            modelBuilder.Entity<JobPostingSkill>()
                .HasKey(ss => new { ss.SkillId, ss.JobPostingId });

            modelBuilder.Entity<JobPostingFavorite>()
                   .HasKey(ss => new { ss.JobPostingId, ss.SubscriberId });

            // Add global query filter for enrollments 
            modelBuilder.Entity<Enrollment>().HasQueryFilter(p => p.IsDeleted == 0);


            modelBuilder.Entity<JobApplication>()
                .Property(ja => ja.JobApplicationStatusId)
                .HasDefaultValue(1);

            //added for foreign key relationship
            modelBuilder.Entity<JobReferral>()
                .HasOne<Subscriber>()
                .WithMany()
                .HasForeignKey(jr => jr.ReferralId);

            modelBuilder.Entity<JobReferral>()
               .HasOne<Subscriber>()
               .WithMany()
               .HasForeignKey(jr => jr.RefereeId)
               .IsRequired(false);

            modelBuilder.Entity<SubscriberNotes>()
              .HasOne<Subscriber>()
              .WithMany()
              .HasForeignKey(sn => sn.SubscriberId);

            modelBuilder.Entity<SubscriberNotes>()
                .HasOne<Recruiter>()
                .WithMany()
                .HasForeignKey(sn => sn.RecruiterId);

            modelBuilder.Entity<Notification>().HasQueryFilter(n => n.IsDeleted == 0 && (n.ExpirationDate > DateTime.UtcNow || n.ExpirationDate == null));
            modelBuilder.Entity<SubscriberFile>().HasQueryFilter(n => n.IsDeleted == 0);
            modelBuilder.Entity<Group>().HasQueryFilter(g => g.IsDeleted == 0);
            modelBuilder.Entity<SubscriberGroup>().HasQueryFilter(sg => sg.IsDeleted == 0);
            modelBuilder.Entity<GroupPartner>().HasQueryFilter(gp => gp.IsDeleted == 0);

            modelBuilder.Entity<Group>()
                .Property(g => g.IsLeavable)
                .HasDefaultValue(1);

            modelBuilder.Entity<Subscriber>()
                .Property(s => s.NotificationEmailsEnabled)
                .HasDefaultValue(true);


            modelBuilder.Entity<NotificationGroup>()
               .HasIndex(p => new { p.NotificationGroupId, p.GroupId })
               .HasName("UIX_NotificationGroup_Group")
               .IsUnique(true);


        }
    }
}

