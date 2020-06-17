using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly UpDiddyDbContext _dbContext;
        private ICountryRepository _countryRepository;
        private IStateRepository _stateRepository;
        private IJobSiteRepository _jobSiteRepository;
        private IJobPageRepository _jobPageRepository;
        private IJobPostingRepository _jobPostingRepository;
        private IJobPostingFavoriteRepository _jobPostingFavoriteRepository;
        private IJobApplicationRepository _jobApplicationRepository;
        private ICompanyRepository _companyRepository;
        private IJobSiteScrapeStatisticRepository _jobSiteScrapeStatisticRepository;
        private IRecruiterActionRepository _recruiterActionRepository;
        private ISubscriberRepository _subscriberRepository;
        private IZeroBounceRepository _zeroBounceRepository;
        private IPartnerContactLeadStatusRepository _partnerContactLeadStatusRepository;
        private IJobCategoryRepository _jobCategoryRepository;
        private IJobReferralRepository _jobReferralRepository;
        private ISubscriberNotesRepository _subscriberNotesRepository;
        private IRecruiterRepository _recruiterRepository;
        private INotificationRepository _notificationRepository;
        private ISubscriberNotificationRepository _subscriberNotificationRepository;
        private IJobPostingAlertRepository _jobPostingAlertRepository;
        private IResumeParseRepository _resumeParseRepository;
        private IResumeParseResultRepository _resumeParseResultRepository;
        private IPartnerReferrerRepository _partnerReferrerRepository;
        private IGroupPartnerRepository _groupPartnerRepository;
        private ISubscriberGroupRepository _subscriberGroupRepository;
        private IGroupRepository _groupRepository;
        private IPartnerContactRepository _partnerContactRepository;
        private IPartnerRepository _partnerRepository;
        private ISubscriberActionRepository _subscriberActionRepository;
        private IEntityTypeRepository _entityTypeRepository;
        private IActionRepository _actionRepository;
        private IContactRepository _contactRepository;
        private IContactTypeRepository _contactTypeRepository;
        private IOfferRepository _offerRepository;
        private ISubscriberFileRepository _subscriberFileRepository;
        private ISkillRepository _skillRepository;
        private IStoredProcedureRepository _storedProcedureRepository;
        private ICourseSiteRepository _courseSiteRepository;
        private ICoursePageRepository _coursePageRepository;
        private ICourseRepository _courseRepository;
        private ICourseVariantRepository _courseVariantRepository;
        private ICourseVariantTypeRepository _courseVariantTypeRepository;
        private ICourseSkillRepository _courseSkillRepository;
        private ITagRepository _tagRepository;
        private ITopicRepository _topicRepository;
        private ITagTopicRepository _tagTopicRepository;
        private ITagCourseRepository _tagCourseRepository;
        private IVendorRepository _vendorRepository;
        private IEnrollmentRepository _enrollmentRepository;
        private IServiceOfferingRepository _serviceOfferingRepository;
        private IServiceOfferingItemRepository _serviceOfferingItemRepository;
        private IServiceOfferingOrderRepository _serviceOfferingOrderRepository;
        private IServiceOfferingPromoCodeRedemptionRepository _serviceOfferingPromoCodeRedemptionRepository;
        private IServiceOfferingPromoCodeRepository _serviceOfferingPromoCodeRepository;
        private IPromoCodeRepository _promoCodeRepository;
        private ITraitifyRepository _traitifyRepository;
        private IFileDownloadTrackerRepository _fileDownloadTrackerRepository;
        private IPartnerTypeRepository _partnerTypeRepository;
        private IJobPostingSkillRepository _jobPostingSkillRepository;
        private ICampaignPartnerContactRepository _campaignParnerContactRepository;
        private ICampaignRepository _campaignRepository;
        private ISubscriberWorkHistoryRepository _subscriberWorkHistoryRepository;
        private ISubscriberSkillRepository _subscriberSkillRepository;
        private ISubscriberEducationHistoryRepository _subscriberEducationHistoryRepository;
        private IIndustryRepository _industryRepository;
        private ISecurityClearanceRepository _securityClearanceRepository;
        private IEmploymentTypeRepository _employmentTypeRepository;
        private IEducationalDegreeRepository _educationalDegreeRepository;
        private IEducationalDegreeTypeRepository _educationalDegreeTypeRepository;
        private IEducationalInstitutionRepository _educationalInstitutionRepository;
        private IEducationLevelRepository _educationLevelRepository;
        private IExperienceLevelRepository _experienceLevelRepository;
        private ICompensationTypeRepository _compensationTypeRepository;
        private IRecruiterCompanyRepository _recruiterCompanyRepository;
        private ITraitifyCourseTopicBlendMappingRepository _traitifyCourseTopicBlendMappingRepository;
        private ICourseFavoriteRepository _courseFavoriteRepository;
        private ISubscriberProfileStagingStoreRepository _subscriberProfileStagingRepository;
        private ITalentFavoriteRepository _talentFavoriteRepository;
        private IPasswordResetRequestRepository _passwordResetRequestRepository;
        private ICourseLevelRepository _courseLevelRepository;
        private ICourseReferralRepository _courseReferralRepository;
        private INotificationGroupRepository _notificationGroupRepository;
        private readonly IConfiguration _configuration;
        private ISendGridEventRepository _sendGridEventRepository;
        private IHiringSolvedResumeParseRepository _hiringSolvedResumeParseRepository;
        private ISovrenParseStatisticRepository _sovrenParseStatisticRepository;
        private ICityRepository _cityRepository;
        private IPostalRepository _postalRepository;
        private IProfileRepository _profileRepository;
        private IAzureIndexStatusRepository _azureIndexStatusRepository;
        private IWishlistRepository _wishlistRepository;
        private ICommentRepository _commentRepository;
        private IEmailTemplateRepository _emailTemplateRepository;
        private IHiringManagerRepository _hiringManagerRepository;
        private IPipelineRepository _pipelineRepository;
        private IInterviewRequestRepository _interviewRequestRepository;
        private ICrosschqRepository _crosschqRepository;
        private ICommuteDistancesRepository _commuteDistancesRepository;

        public RepositoryWrapper(UpDiddyDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public IPasswordResetRequestRepository PasswordResetRequestRepository
        {
            get
            {
                if (_passwordResetRequestRepository == null)
                {
                    _passwordResetRequestRepository = new PasswordResetRequestRepository(_dbContext);
                }
                return _passwordResetRequestRepository;
            }
        }

        public ICountryRepository Country
        {
            get
            {
                if (_countryRepository == null)
                {
                    _countryRepository = new CountryRepository(_dbContext, State);
                }
                return _countryRepository;
            }
        }

        public IStateRepository State
        {
            get
            {
                if (_stateRepository == null)
                {
                    _stateRepository = new StateRepository(_dbContext);
                }
                return _stateRepository;
            }
        }

        public IJobSiteRepository JobSite
        {
            get
            {
                if (_jobSiteRepository == null)
                {
                    _jobSiteRepository = new JobSiteRepository(_dbContext);
                }
                return _jobSiteRepository;
            }
        }

        public IJobPageRepository JobPage
        {
            get
            {
                if (_jobPageRepository == null)
                {
                    _jobPageRepository = new JobPageRepository(_dbContext);
                }
                return _jobPageRepository;
            }
        }

        public IJobSiteScrapeStatisticRepository JobSiteScrapeStatistic
        {
            get
            {
                if (_jobSiteScrapeStatisticRepository == null)
                {
                    _jobSiteScrapeStatisticRepository = new JobSiteScrapeStatisticRepository(_dbContext);
                }
                return _jobSiteScrapeStatisticRepository;
            }
        }

        public IJobPostingRepository JobPosting
        {
            get
            {
                if (_jobPostingRepository == null)
                {
                    _jobPostingRepository = new JobPostingRepository(_dbContext);
                }
                return _jobPostingRepository;
            }
        }

        public IJobPostingFavoriteRepository JobPostingFavorite
        {
            get
            {
                if (_jobPostingFavoriteRepository == null)
                {
                    _jobPostingFavoriteRepository = new JobPostingFavoriteRepository(_dbContext, _configuration);
                }
                return _jobPostingFavoriteRepository;
            }
        }

        public IJobApplicationRepository JobApplication
        {
            get
            {
                if (_jobApplicationRepository == null)
                {
                    _jobApplicationRepository = new JobApplicationRepository(_dbContext);
                }
                return _jobApplicationRepository;
            }
        }

        public ICommuteDistancesRepository CommuteDistancesRepository
        {
            get
            {
                if (_commuteDistancesRepository == null)
                {
                    _commuteDistancesRepository = new CommuteDistancesRepository(_dbContext);
                }
                return _commuteDistancesRepository;
            }
        }

        public ICompanyRepository Company
        {
            get
            {
                if (_companyRepository == null)
                {
                    _companyRepository = new CompanyRepository(_dbContext);
                }
                return _companyRepository;
            }
        }

        public ICrosschqRepository CrosschqRepository
        {
            get
            {
                if (_crosschqRepository == null)
                {
                    _crosschqRepository = new CrosschqRepository(_dbContext);
                }
                return _crosschqRepository;
            }
        }


        public IRecruiterActionRepository RecruiterActionRepository
        {
            get
            {
                if (_recruiterActionRepository == null)
                {
                    _recruiterActionRepository = new RecruiterActionRepository(_dbContext);
                }
                return _recruiterActionRepository;
            }
        }

        public IZeroBounceRepository ZeroBounceRepository
        {
            get
            {
                if (_zeroBounceRepository == null)
                {
                    _zeroBounceRepository = new ZeroBounceRepository(_dbContext);
                }
                return _zeroBounceRepository;
            }
        }

        public IPartnerContactLeadStatusRepository PartnerContactLeadStatusRepository
        {
            get
            {
                if (_partnerContactLeadStatusRepository == null)
                {
                    _partnerContactLeadStatusRepository = new PartnerContactLeadStatusRepository(_dbContext);
                }
                return _partnerContactLeadStatusRepository;
            }
        }

        public IJobCategoryRepository JobCategoryRepository
        {
            get
            {
                if (_jobCategoryRepository == null)
                {
                    _jobCategoryRepository = new JobCategoryRepository(_dbContext);
                }
                return _jobCategoryRepository;
            }
        }


        public ISubscriberRepository SubscriberRepository
        {
            get
            {
                if (_subscriberRepository == null)
                {
                    _subscriberRepository = new SubscriberRepository(_dbContext, SubscriberGroupRepository, GroupPartnerRepository, PartnerRepository, StoredProcedureRepository);
                }
                return _subscriberRepository;
            }
        }

        public IJobReferralRepository JobReferralRepository
        {
            get
            {
                if (_jobReferralRepository == null)
                {
                    _jobReferralRepository = new JobReferralRepository(_dbContext);
                }
                return _jobReferralRepository;
            }
        }

        public IJobPostingAlertRepository JobPostingAlertRepository
        {
            get
            {
                if (_jobPostingAlertRepository == null)
                {
                    _jobPostingAlertRepository = new JobPostingAlertRepository(_dbContext);
                }
                return _jobPostingAlertRepository;
            }
        }

        public ISubscriberNotesRepository SubscriberNotesRepository
        {
            get
            {
                if (_subscriberNotesRepository == null)
                {
                    _subscriberNotesRepository = new SubscriberNotesRepository(_dbContext);
                }
                return _subscriberNotesRepository;
            }
        }

        public IRecruiterRepository RecruiterRepository
        {
            get
            {
                if (_recruiterRepository == null)
                {
                    _recruiterRepository = new RecruiterRepository(_dbContext);
                }
                return _recruiterRepository;
            }
        }

        public INotificationRepository NotificationRepository
        {
            get
            {
                if (_notificationRepository == null)
                {
                    _notificationRepository = new NotificationRepository(_dbContext);
                }
                return _notificationRepository;
            }
        }

        public ISubscriberNotificationRepository SubscriberNotificationRepository
        {
            get
            {
                if (_subscriberNotificationRepository == null)
                {
                    _subscriberNotificationRepository = new SubscriberNotificationRepository(_dbContext);
                }
                return _subscriberNotificationRepository;
            }
        }


        public IResumeParseRepository ResumeParseRepository
        {
            get
            {
                if (_resumeParseRepository == null)
                {
                    _resumeParseRepository = new ResumeParseRepository(_dbContext);
                }
                return _resumeParseRepository;
            }
        }

        public IResumeParseResultRepository ResumeParseResultRepository
        {
            get
            {
                if (_resumeParseResultRepository == null)
                {
                    _resumeParseResultRepository = new ResumeParseResultRespository(_dbContext);
                }
                return _resumeParseResultRepository;
            }
        }

        public IPartnerReferrerRepository PartnerReferrerRepository
        {
            get
            {
                if (_partnerReferrerRepository == null)
                {
                    _partnerReferrerRepository = new PartnerReferrerRepository(_dbContext);
                }
                return _partnerReferrerRepository;
            }
        }

        public IGroupPartnerRepository GroupPartnerRepository
        {
            get
            {
                if (_groupPartnerRepository == null)
                {
                    _groupPartnerRepository = new GroupPartnerRepository(_dbContext);
                }
                return _groupPartnerRepository;
            }
        }

        public ISubscriberGroupRepository SubscriberGroupRepository
        {
            get
            {
                if (_subscriberGroupRepository == null)
                {
                    _subscriberGroupRepository = new SubscriberGroupRepository(_dbContext);
                }
                return _subscriberGroupRepository;
            }
        }

        public IGroupRepository GroupRepository
        {
            get
            {
                if (_groupRepository == null)
                {
                    _groupRepository = new GroupRepository(_dbContext);
                }
                return _groupRepository;
            }
        }

        public IPartnerContactRepository PartnerContactRepository
        {
            get
            {
                if (_partnerContactRepository == null)
                {
                    _partnerContactRepository = new PartnerContactRepository(_dbContext, ContactRepository);
                }
                return _partnerContactRepository;
            }
        }

        public IPartnerRepository PartnerRepository
        {
            get
            {
                if (_partnerRepository == null)
                {
                    _partnerRepository = new PartnerRepository(_dbContext);
                }
                return _partnerRepository;
            }
        }

        public ISubscriberActionRepository SubscriberActionRepository
        {
            get
            {
                if (_subscriberActionRepository == null)
                {
                    _subscriberActionRepository = new SubscriberActionRepository(_dbContext);
                }
                return _subscriberActionRepository;
            }
        }

        public IEntityTypeRepository EntityTypeRepository
        {
            get
            {
                if (_entityTypeRepository == null)
                {
                    _entityTypeRepository = new EntityTypeRepository(_dbContext);
                }
                return _entityTypeRepository;
            }
        }

        public IActionRepository ActionRepository
        {
            get
            {
                if (_actionRepository == null)
                {
                    _actionRepository = new ActionRepository(_dbContext);
                }
                return _actionRepository;
            }
        }

        public IContactRepository ContactRepository
        {
            get
            {
                if (_contactRepository == null)
                {
                    _contactRepository = new ContactRepository(_dbContext);
                }
                return _contactRepository;
            }
        }

        public IContactTypeRepository ContactTypeRepository
        {
            get
            {
                if (_contactTypeRepository == null)
                {
                    _contactTypeRepository = new ContactTypeRepository(_dbContext);
                }
                return _contactTypeRepository;
            }
        }

        public IOfferRepository Offer
        {
            get
            {
                if (_offerRepository == null)
                {
                    _offerRepository = new OfferRepository(_dbContext);
                }
                return _offerRepository;
            }
        }

        public ISubscriberFileRepository SubscriberFileRepository
        {
            get
            {
                if (_subscriberFileRepository == null)
                {
                    _subscriberFileRepository = new SubscriberFileRepository(_dbContext);
                }
                return _subscriberFileRepository;
            }
        }


        public ISkillRepository SkillRepository
        {
            get
            {
                if (_skillRepository == null)
                {
                    _skillRepository = new SkillRepository(_dbContext);
                }
                return _skillRepository;
            }
        }

        public IStoredProcedureRepository StoredProcedureRepository
        {
            get
            {
                if (_storedProcedureRepository == null)
                {
                    _storedProcedureRepository = new StoredProcedureRepository(_dbContext);
                }
                return _storedProcedureRepository;
            }
        }

        public ICourseSiteRepository CourseSite
        {
            get
            {
                if (_courseSiteRepository == null)
                {
                    _courseSiteRepository = new CourseSiteRepository(_dbContext);
                }
                return _courseSiteRepository;
            }
        }

        public ICoursePageRepository CoursePage
        {
            get
            {
                if (_coursePageRepository == null)
                {
                    _coursePageRepository = new CoursePageRepository(_dbContext);
                }
                return _coursePageRepository;
            }
        }

        public ICourseRepository Course
        {
            get
            {
                if (_courseRepository == null)
                {
                    _courseRepository = new CourseRepository(_dbContext);
                }
                return _courseRepository;
            }
        }

        public ICourseVariantRepository CourseVariant
        {
            get
            {
                if (_courseVariantRepository == null)
                {
                    _courseVariantRepository = new CourseVariantRepository(_dbContext);
                }
                return _courseVariantRepository;
            }
        }

        public ICourseVariantTypeRepository CourseVariantType
        {
            get
            {
                if (_courseVariantTypeRepository == null)
                {
                    _courseVariantTypeRepository = new CourseVariantTypeRepository(_dbContext);
                }
                return _courseVariantTypeRepository;
            }
        }

        public ICourseSkillRepository CourseSkill
        {
            get
            {
                if (_courseSkillRepository == null)
                {
                    _courseSkillRepository = new CourseSkillRepository(_dbContext);
                }
                return _courseSkillRepository;
            }
        }

        public ITagRepository Tag
        {
            get
            {
                if (_tagRepository == null)
                {
                    _tagRepository = new TagRepository(_dbContext);
                }
                return _tagRepository;
            }
        }

        public ITopicRepository Topic
        {
            get
            {
                if (_topicRepository == null)
                {
                    _topicRepository = new TopicRepository(_dbContext);
                }
                return _topicRepository;
            }
        }

        public ITagTopicRepository TagTopic
        {
            get
            {
                if (_tagTopicRepository == null)
                {
                    _tagTopicRepository = new TagTopicRepository(_dbContext);
                }
                return _tagTopicRepository;
            }
        }

        public ITagCourseRepository TagCourse
        {
            get
            {
                if (_tagCourseRepository == null)
                {
                    _tagCourseRepository = new TagCourseRepository(_dbContext);
                }
                return _tagCourseRepository;
            }
        }

        public IVendorRepository Vendor
        {
            get
            {
                if (_vendorRepository == null)
                {
                    _vendorRepository = new VendorRepository(_dbContext);
                }
                return _vendorRepository;
            }
        }

        public IEnrollmentRepository EnrollmentRepository
        {
            get
            {
                if (_enrollmentRepository == null)
                {
                    _enrollmentRepository = new EnrollmentRepository(_dbContext);
                }
                return _enrollmentRepository;
            }
        }


        public IServiceOfferingRepository ServiceOfferingRepository
        {
            get
            {
                if (_serviceOfferingRepository == null)
                {
                    _serviceOfferingRepository = new ServiceOfferingRepository(_dbContext);
                }
                return _serviceOfferingRepository;
            }
        }

        public IServiceOfferingItemRepository ServiceOfferingItemRepository
        {
            get
            {
                if (_serviceOfferingItemRepository == null)
                {
                    _serviceOfferingItemRepository = new ServiceOfferingItemRepository(_dbContext);
                }
                return _serviceOfferingItemRepository;
            }
        }

        public IServiceOfferingOrderRepository ServiceOfferingOrderRepository
        {
            get
            {
                if (_serviceOfferingOrderRepository == null)
                {
                    _serviceOfferingOrderRepository = new ServiceOfferingOrderRepository(_dbContext);
                }
                return _serviceOfferingOrderRepository;
            }
        }


        public IServiceOfferingPromoCodeRedemptionRepository ServiceOfferingPromoCodeRedemptionRepository
        {
            get
            {
                if (_serviceOfferingPromoCodeRedemptionRepository == null)
                {
                    _serviceOfferingPromoCodeRedemptionRepository = new ServiceOfferingPromoCodeRedemptionRepository(_dbContext);
                }
                return _serviceOfferingPromoCodeRedemptionRepository;
            }
        }

        public IServiceOfferingPromoCodeRepository ServiceOfferingPromoCodeRepository
        {
            get
            {
                if (_serviceOfferingPromoCodeRepository == null)
                {
                    _serviceOfferingPromoCodeRepository = new ServiceOfferingPromoCodeRepository(_dbContext);
                }
                return _serviceOfferingPromoCodeRepository;
            }
        }

        public IPromoCodeRepository PromoCodeRepository
        {
            get
            {
                if (_promoCodeRepository == null)
                {
                    _promoCodeRepository = new PromoCodeRepository(_dbContext);
                }
                return _promoCodeRepository;
            }
        }

        public ITraitifyRepository TraitifyRepository
        {
            get
            {
                if (_traitifyRepository == null)
                {
                    _traitifyRepository = new TraitifyRepository(_dbContext);
                }
                return _traitifyRepository;
            }
        }

        public IFileDownloadTrackerRepository FileDownloadTrackerRepository
        {
            get
            {
                if (_fileDownloadTrackerRepository == null)
                {
                    _fileDownloadTrackerRepository = new FileDownloadTrackerRepository(_dbContext);
                }
                return _fileDownloadTrackerRepository;
            }
        }


        public IPartnerTypeRepository PartnerTypeRepository
        {
            get
            {
                if (_partnerTypeRepository == null)
                {
                    _partnerTypeRepository = new PartnerTypeRepository(_dbContext);
                }
                return _partnerTypeRepository;
            }

        }

        public IJobPostingSkillRepository JobPostingSkillRepository
        {
            get
            {
                if (_jobPostingSkillRepository == null)
                {
                    _jobPostingSkillRepository = new JobPostingSkillRepository(_dbContext);
                }
                return _jobPostingSkillRepository;
            }
        }

        public ICampaignPartnerContactRepository CampaignPartnerContactRepository
        {
            get
            {
                if (_campaignParnerContactRepository == null)
                {
                    _campaignParnerContactRepository = new CampaignPartnerContactRepository(_dbContext);
                }
                return _campaignParnerContactRepository;
            }
        }

        public ICampaignRepository CampaignRepository
        {
            get
            {
                if (_campaignRepository == null)
                {
                    _campaignRepository = new CampaignRepository(_dbContext);
                }
                return _campaignRepository;
            }
        }

        public ISubscriberWorkHistoryRepository SubscriberWorkHistoryRepository
        {
            get
            {
                if (_subscriberWorkHistoryRepository == null)
                {
                    _subscriberWorkHistoryRepository = new SubscriberWorkHistoryRepository(_dbContext);
                }
                return _subscriberWorkHistoryRepository;
            }
        }

        public ISubscriberSkillRepository SubscriberSkillRepository
        {
            get
            {
                if (_subscriberSkillRepository == null)
                {
                    _subscriberSkillRepository = new SubscriberSkillRepository(_dbContext);
                }
                return _subscriberSkillRepository;
            }
        }

        public ISubscriberEducationHistoryRepository SubscriberEducationHistoryRepository
        {
            get
            {
                if (_subscriberEducationHistoryRepository == null)
                {
                    _subscriberEducationHistoryRepository = new SubscriberEducationHistoryRepository(_dbContext);
                }
                return _subscriberEducationHistoryRepository;
            }
        }

        public IIndustryRepository IndustryRepository
        {
            get
            {
                if (_industryRepository == null)
                {
                    _industryRepository = new IndustryRepository(_dbContext);
                }
                return _industryRepository;
            }
        }

        public ISecurityClearanceRepository SecurityClearanceRepository
        {
            get
            {
                if (_securityClearanceRepository == null)
                {
                    _securityClearanceRepository = new SecurityClearanceRepository(_dbContext);
                }
                return _securityClearanceRepository;
            }
        }

        public IEmploymentTypeRepository EmploymentTypeRepository
        {
            get
            {
                if (_employmentTypeRepository == null)
                {
                    _employmentTypeRepository = new EmploymentTypeRepository(_dbContext);
                }
                return _employmentTypeRepository;
            }
        }

        public IEducationalDegreeRepository EducationalDegreeRepository
        {
            get
            {
                if (_educationalDegreeRepository == null)
                {
                    _educationalDegreeRepository = new EducationalDegreeRepository(_dbContext);
                }
                return _educationalDegreeRepository;
            }
        }

        public IEducationalDegreeTypeRepository EducationalDegreeTypeRepository
        {
            get
            {
                if (_educationalDegreeTypeRepository == null)
                {
                    _educationalDegreeTypeRepository = new EducationalDegreeTypeRepository(_dbContext);
                }
                return _educationalDegreeTypeRepository;
            }
        }

        public IEducationalInstitutionRepository EducationalInstitutionRepository
        {
            get
            {
                if (_educationalInstitutionRepository == null)
                {
                    _educationalInstitutionRepository = new EducationalInstitutionRepository(_dbContext);
                }
                return _educationalInstitutionRepository;
            }
        }

        public IEducationLevelRepository EducationLevelRepository
        {
            get
            {
                if (_educationLevelRepository == null)
                {
                    _educationLevelRepository = new EducationLevelRepository(_dbContext);
                }
                return _educationLevelRepository;
            }
        }

        public IExperienceLevelRepository ExperienceLevelRepository
        {
            get
            {
                if (_experienceLevelRepository == null)
                {
                    _experienceLevelRepository = new ExperienceLevelRepository(_dbContext);
                }
                return _experienceLevelRepository;
            }
        }

        public ICompensationTypeRepository CompensationTypeRepository
        {
            get
            {
                if (_compensationTypeRepository == null)
                {
                    _compensationTypeRepository = new CompensationTypeRepository(_dbContext);
                }
                return _compensationTypeRepository;
            }
        }

        public IRecruiterCompanyRepository RecruiterCompanyRepository
        {
            get
            {
                if (_recruiterCompanyRepository == null)
                {
                    _recruiterCompanyRepository = new RecruiterCompanyRepository(_dbContext);
                }
                return _recruiterCompanyRepository;
            }
        }

        public ITraitifyCourseTopicBlendMappingRepository TraitifyCourseTopicBlendMappingRepository
        {
            get
            {
                if (_traitifyCourseTopicBlendMappingRepository == null)
                {
                    _traitifyCourseTopicBlendMappingRepository = new TraitifyCourseTopicBlendMappingRepository(_dbContext);
                }
                return _traitifyCourseTopicBlendMappingRepository;
            }
        }

        public ICourseFavoriteRepository CourseFavoriteRepository
        {
            get
            {
                if (_courseFavoriteRepository == null)
                {
                    _courseFavoriteRepository = new CourseFavoriteRepository(_dbContext);
                }
                return _courseFavoriteRepository;
            }
        }

        public ISubscriberProfileStagingStoreRepository SubscriberProfileStagingStoreRepository
        {
            get
            {
                if (_subscriberProfileStagingRepository == null)
                {
                    _subscriberProfileStagingRepository = new SubscriberProfileStagingStoreRepository(_dbContext);
                }
                return _subscriberProfileStagingRepository;
            }

        }


        public ITalentFavoriteRepository TalentFavoriteRepository
        {
            get
            {
                if (_talentFavoriteRepository == null)
                {
                    _talentFavoriteRepository = new TalentFavoriteRepository(_dbContext);
                }
                return _talentFavoriteRepository;
            }
        }

        public ICourseLevelRepository CourseLevelRepository
        {
            get
            {
                if (_courseLevelRepository == null)
                {
                    _courseLevelRepository = new CourseLevelRepository(_dbContext);
                }
                return _courseLevelRepository;
            }
        }


        public ICourseReferralRepository CourseReferralRepository
        {
            get
            {
                if (_courseReferralRepository == null)
                {
                    _courseReferralRepository = new CourseReferralRepository(_dbContext);
                }
                return _courseReferralRepository;
            }
        }

        public ISendGridEventRepository SendGridEventRepository
        {
            get
            {
                if (_sendGridEventRepository == null)
                {
                    _sendGridEventRepository = new SendGridEventRepository(_dbContext);
                }
                return _sendGridEventRepository;
            }
        }


        public INotificationGroupRepository NotificationGroupRepository
        {
            get
            {
                if (_notificationGroupRepository == null)
                {
                    _notificationGroupRepository = new NotificationGroupRepository(_dbContext);
                }
                return _notificationGroupRepository;
            }
        }


        public IHiringSolvedResumeParseRepository HiringSolvedResumeParseRepository
        {
            get
            {
                if (_hiringSolvedResumeParseRepository == null)
                {
                    _hiringSolvedResumeParseRepository = new HiringSolvedResumeParseRepository(_dbContext);
                }
                return _hiringSolvedResumeParseRepository;
            }
        }

        public ISovrenParseStatisticRepository SovrenParseStatisticRepository
        {
            get
            {
                if (_sovrenParseStatisticRepository == null)
                {
                    _sovrenParseStatisticRepository = new SovrenParseStatisticRepository(_dbContext);
                }
                return _sovrenParseStatisticRepository;
            }
        }

        public ICityRepository CityRepository
        {
            get
            {
                if (_cityRepository == null)
                {
                    _cityRepository = new CityRepository(_dbContext);
                }
                return _cityRepository;
            }
        }


        public IPostalRepository PostalRepository
        {
            get
            {
                if (_postalRepository== null)
                {
                    _postalRepository = new PostalRepository(_dbContext);
                }
                return _postalRepository;
            }
        }

        public IProfileRepository ProfileRepository
        {
            get
            {
                if (_profileRepository == null)
                {
                    _profileRepository = new ProfileRepository(_dbContext);
                }
                return _profileRepository;
            }
        }

        public IAzureIndexStatusRepository AzureIndexStatusRepository
        {
            get
            {
                if (_azureIndexStatusRepository == null)
                {
                    _azureIndexStatusRepository = new AzureIndexStatusRepository(_dbContext);
                }
                return _azureIndexStatusRepository;
            }
        }

        public IWishlistRepository WishlistRepository
        {
            get
            {
                if (_wishlistRepository == null)
                {
                    _wishlistRepository = new WishlistRepository(_dbContext);
                }
                return _wishlistRepository;
            }
        }

        public ICommentRepository CommentRepository
        {
            get
            {
                if (_commentRepository == null)
                {
                    _commentRepository = new CommentRepository(_dbContext);
                }
                return _commentRepository;
            }
        }

        public IEmailTemplateRepository EmailTemplateRepository
        {
            get
            {
                if (_emailTemplateRepository == null)
                {
                    _emailTemplateRepository = new EmailTemplateRepository(_dbContext);
                }
                return _emailTemplateRepository;
            }
        }

        public IPipelineRepository PipelineRepository
        {
            get
            {
                if (_pipelineRepository == null)
                {
                    _pipelineRepository = new PipelineRepository(_dbContext);
                }
                return _pipelineRepository;
            }
        }

        public IHiringManagerRepository HiringManagerRepository
        {
            get
            {
                if (_hiringManagerRepository == null)
                {
                    _hiringManagerRepository = new HiringManagerRepository(_dbContext);
                }
                return _hiringManagerRepository;
            }
        }

        public IInterviewRequestRepository InterviewRequestRepository
        {
            get
            {
                if (_interviewRequestRepository == null)
                {
                    _interviewRequestRepository = new InterviewRequestRepository(_dbContext);
                }
                return _interviewRequestRepository;
            }
        }

        public async Task SaveAsync()
        {
            var modifiedEntities = _dbContext.ChangeTracker.Entries()
           .Where(p => p.State == EntityState.Modified).ToList();
            var now = DateTime.UtcNow;
            foreach (var change in modifiedEntities)
            {
                var modifyDateProp = change.Entity.GetType().GetProperty("ModifyDate");
                modifyDateProp.SetValue(change.Entity, now);
            }
            await this._dbContext.SaveChangesAsync();
        }
    }
}