namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IRepositoryWrapper
    {
        ICountryRepository Country { get; }
        IStateRepository State { get; }
        IJobSiteRepository JobSite { get; }
        IJobPageRepository JobPage { get; }
        IJobSiteScrapeStatisticRepository JobSiteScrapeStatistic { get; }
        IJobCategoryRepository JobCategoryRepository { get; }
        IJobPostingRepository JobPosting { get; }
        IJobPostingFavoriteRepository JobPostingFavorite { get; }
        IJobApplicationRepository JobApplication { get; }
        ICompanyRepository Company { get; }
        IRecruiterActionRepository RecruiterActionRepository { get; }
        ISubscriberRepository Subscriber { get; }
        IZeroBounceRepository ZeroBounceRepository { get; }
        IPartnerContactLeadStatusRepository PartnerContactLeadStatusRepository { get; }
        ISubscriberRepository SubscriberRepository { get; }
        IJobReferralRepository JobReferralRepository { get; }
        ISubscriberNotesRepository SubscriberNotesRepository { get; }
        IRecruiterRepository RecruiterRepository { get; }
        INotificationRepository NotificationRepository { get; }
        ISubscriberNotificationRepository SubscriberNotificationRepository { get; }
        IJobPostingAlertRepository JobPostingAlertRepository { get; }
        IResumeParseRepository ResumeParseRepository { get; }
        IResumeParseResultRepository ResumeParseResultRepository { get; }
        IPartnerReferrerRepository PartnerReferrerRepository { get; }
        IGroupPartnerRepository GroupPartnerRepository { get; }
        IGroupRepository GroupRepository { get; }
        ISubscriberGroupRepository SubscriberGroupRepository { get; }
        IPartnerContactRepository PartnerContactRepository { get; }
        IPartnerRepository PartnerRepository { get; }
        ISubscriberActionRepository SubscriberActionRepository { get; }
        IEntityTypeRepository EntityTypeRepository { get; }
        IActionRepository ActionRepository { get; }
        IContactRepository ContactRepository { get; }
        IOfferRepository Offer { get; }
        ISubscriberFileRepository SubscriberFileRepository { get; }
        ISkillRepository SkillRepository { get; }
        IStoredProcedureRepository StoredProcedureRepository { get;  }
        ICourseSiteRepository CourseSite { get; }
        ICoursePageRepository CoursePage { get; }
        ICourseRepository Course { get; }
        ICourseSkillRepository CourseSkill { get; }
        ICourseVariantRepository CourseVariant { get; }
        ICourseVariantTypeRepository CourseVariantType { get; }
        ITagRepository Tag { get; }
        ITagTopicRepository TagTopic { get; }
        ITopicRepository Topic { get; }
        ITagCourseRepository TagCourse { get; }
        IVendorRepository Vendor { get; }
        IEnrollmentRepository EnrollmentRepository { get; }
    }
}