using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using UpDiddyLib.Helpers;
using System.Text.RegularExpressions;
using System.Web;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNet.OData.Query;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberService : ISubscriberService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private IB2CGraph _graphClient { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;

        public SubscriberService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IB2CGraph graphClient,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _graphClient = graphClient;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<SubscriberFile> AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream, bool parseResume = false)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    SubscriberFile resume = await _AddResumeAsync(subscriber, fileName, fileStream);
                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    if (parseResume)
                        BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(subscriber, resume));

                    return resume;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

        }

        public async Task<Subscriber> CreateSubscriberAsync(Guid partnerContactGuid, SignUpDto signUpDto)
        {
            var partnerContact = await _db.PartnerContact
                .Include(pc => pc.Contact)
                .Where(pc => pc.PartnerContactGuid == partnerContactGuid && pc.IsDeleted == 0 && pc.Contact.IsDeleted == 0)
                .FirstOrDefaultAsync();

            Campaign campaign = await _db.Campaign
                .Where(camp => camp.CampaignGuid.Equals(signUpDto.campaignGuid)
                    && camp.IsDeleted == 0
                    && camp.StartDate <= DateTime.UtcNow
                    && (!camp.EndDate.HasValue || camp.EndDate.Value >= DateTime.UtcNow))
                .FirstOrDefaultAsync();

            #region Verify and Check Data
            if (partnerContact == null)
                throw new ArgumentException("Invalid PartnerContact guid.");

            if (campaign == null)
                throw new ArgumentException("Signing up through this campaign is not available at this time.");

            // check email
            if (partnerContact.Contact.Email != signUpDto.email)
                throw new ArgumentException("This offer is only good for the recepient of this email campaign.");

            // check if subscriber is in database
            Subscriber subscriber = await _db.Subscriber.Where(s => s.Email == partnerContact.Contact.Email).FirstOrDefaultAsync();
            if (subscriber != null)
                throw new ArgumentException("Subscriber already exists, please login to continue.");
            #endregion

            User b2cUser = await _CreateB2CUser(signUpDto.email, signUpDto.password);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    Guid subscriberGuid = Guid.Parse(b2cUser.AdditionalData["objectId"].ToString());
                    Subscriber newSubscriber = new Subscriber()
                    {
                        SubscriberGuid = subscriberGuid,
                        ModifyGuid = subscriberGuid,
                        CreateGuid = subscriberGuid,
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        IsDeleted = 0,
                        IsVerified = true,
                        FirstName = partnerContact.Metadata["FirstName"].ToString(),
                        LastName = partnerContact.Metadata["LastName"].ToString(),
                        Email = partnerContact.Contact.Email,
                        PhoneNumber = partnerContact.Metadata["MobilePhone"].ToString(),
                        Address = string.Join(' ', partnerContact.Metadata["Address1"].ToString(), partnerContact.Metadata["Address2"].ToString()),
                        City = partnerContact.Metadata["City"].ToString(),
                        PostalCode = partnerContact.Metadata["PostalCode"].ToString(),
                        State = _db.State.Where(s => s.Name == partnerContact.Metadata["State"].ToString()).FirstOrDefault()
                    };
                    _db.Subscriber.Add(newSubscriber);
                    await _db.SaveChangesAsync();

                    // update google profile 
                    BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(newSubscriber.SubscriberGuid.Value));


                    partnerContact.Contact.SubscriberId = newSubscriber.SubscriberId;
                    CampaignPhase campaignPhase = CampaignPhaseFactory.GetCampaignPhaseByNameOrInitial(_db, campaign.CampaignId, signUpDto.campaignPhase);
                    _db.PartnerContactAction.Add(new PartnerContactAction()
                    {
                        ActionId = 3, // todo: use constants or enum or something
                        CampaignId = campaign.CampaignId,
                        PartnerContactId = partnerContact.PartnerContactId,
                        PartnerContactActionGuid = Guid.NewGuid(),
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        OccurredDate = DateTime.UtcNow,
                        CampaignPhaseId = campaignPhase.CampaignPhaseId
                    });

                    var file = await _db.PartnerContactFile.Where(e => e.PartnerContactId == partnerContact.PartnerContactId)
                        .OrderByDescending(e => e.CreateDate)
                        .FirstOrDefaultAsync();

                    if (file != null)
                    {
                        var bytes = Convert.FromBase64String(file.Base64EncodedData);
                        var contents = new MemoryStream(bytes);
                        await _AddResumeAsync(newSubscriber, file.Name, contents);
                    }

                    // associate the contact with the subscriber
                    var contact = _db.Contact.Where(c => c.ContactId == partnerContact.ContactId).FirstOrDefault();
                    contact.SubscriberId = partnerContact.Contact.SubscriberId;

                    // assign subscriber source for campaigns
                    SubscriberProfileStagingStore attribution = new SubscriberProfileStagingStore()
                    {
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        CreateGuid = Guid.Empty,
                        SubscriberId = partnerContact.Contact.SubscriberId.Value,
                        ProfileSource = UpDiddyLib.Helpers.Constants.DataSource.CareerCircle,
                        IsDeleted = 0,
                        ProfileFormat = UpDiddyLib.Helpers.Constants.DataFormat.Json,
                        ProfileData = JsonConvert.SerializeObject(new { source = "campaign-sign-up", referer = campaign.Name })
                    };
                    _db.SubscriberProfileStagingStore.Add(attribution);

                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

                return subscriber;
            }
        }

        /// <summary>
        /// Creates B2C User if one doesn't exist already.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<User> _CreateB2CUser(string email, string password)
        {
            Microsoft.Graph.User user = await _graphClient.GetUserBySignInEmail(email);
            if (user == null)
                user = await _graphClient.CreateUser(email, email, password);

            return user;
        }

        /// <summary>
        /// Uploads file to cloud storage and associates the file with Subscriber Entity.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private async Task<SubscriberFile> _AddResumeAsync(Subscriber subscriber, string fileName, Stream fileStream)
        {
            string blobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriber.SubscriberGuid, "resume"), fileName, fileStream);
            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = blobName,
                ModifyGuid = subscriber.SubscriberGuid.Value,
                CreateGuid = subscriber.SubscriberGuid.Value,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                SubscriberId = subscriber.SubscriberId
            };

            // check to see if file is already in the system, if there is a file in the system in already then delete it
            // todo: refactor as part of multiple file upload/management system
            // todo: move logic to OnDelete event or somewhere centralized and run as a transaction somehow
            if (subscriber.SubscriberFile.Count > 0)
            {
                SubscriberFile oldFile = subscriber.SubscriberFile.Last();
                await _cloudStorage.DeleteFileAsync(oldFile.BlobName);
                // subscriber.SubscriberFile.Remove(oldFile);
                oldFile.IsDeleted = 1;

            }

            subscriber.SubscriberFile.Add(subscriberFileResume);

            return subscriberFileResume;
        }

        public async Task<Stream> GetResumeAsync(Subscriber subscriber)
        {

            SubscriberFile file = subscriber.SubscriberFile.Where(
                f => f.SubscriberFileGuid.Equals(
                    subscriber.SubscriberFile.FirstOrDefault()?.SubscriberFileGuid)).First();

            if (file == null)
                return null;

            return await _cloudStorage.OpenReadAsync(file.BlobName);
        }

        public async Task<bool> QueueScanResumeJobAsync(Guid subscriberGuid)
        {
            Subscriber subscriber = await _db.Subscriber
                .Where(e => e.IsDeleted == 0 && e.SubscriberGuid == subscriberGuid)
                .Include(e => e.SubscriberFile)
                .FirstOrDefaultAsync();

            SubscriberFile resume = subscriber.SubscriberFile.OrderByDescending(e => e.CreateDate).FirstOrDefault();

            if (resume != null)
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(subscriber, resume));

            return true;
        }

        public async Task<Dictionary<Guid, Guid>> GetSubscriberJobPostingFavoritesByJobGuid(Guid subscriberGuid, List<Guid> jobGuids)
        {
            var subscribers = await _repository.Subscriber.GetAllSubscribersAsync();
            var jobPostingFavorites = await _repository.JobPostingFavorite.GetAllJobPostingFavoritesAsync();
            var jobPostings = await _repository.JobPosting.GetAllJobPostings();
            jobPostings = jobPostings.Where(jp => jobGuids.Contains(jp.JobPostingGuid));

            var query = from jp in jobPostings
                        join favorites in jobPostingFavorites on jp.JobPostingId equals favorites.JobPostingId
                        join sub in subscribers on favorites.SubscriberId equals sub.SubscriberId
                        where (sub.SubscriberGuid == subscriberGuid && favorites.IsDeleted == 0)
                        select new
                        {
                            jobPostingGuid = jp.JobPostingGuid,
                            jobPostingFavoriteGuid = favorites.JobPostingFavoriteGuid
                        };
            var map = query.ToDictionary(x => x.jobPostingGuid, x => x.jobPostingFavoriteGuid);
            return map;
        }
 



        #region subscriber notes
        public async Task SaveSubscriberNotesAsync(SubscriberNotesDto subscriberNotesDto)
        {
            //check if notes exist for subscriber


            if (subscriberNotesDto.SubscriberNotesGuid == null)
            {
                var subscriberNotes = _mapper.Map<SubscriberNotes>(subscriberNotesDto);

                //Assign new GUID for new notes
                subscriberNotes.SubscriberNotesGuid = Guid.NewGuid();

                //get subscriber by subscriberGuid and assign SubscriberId
                var subscriber = await _repository.Subscriber.GetSubscriberByGuidAsync(subscriberNotesDto.SubscriberGuid);
                subscriberNotes.SubscriberId = subscriber.SubscriberId;

                //get subscriber by subscriberGuid  map to recruited and get recruiterId
                //recruiter is also a subscriber
                var recruiter = await _repository.Subscriber.GetSubscriberByGuidAsync(subscriberNotesDto.RecruiterGuid);
                var rec = await _repository.RecruiterRepository.GetRecruiterBySubscriberId(recruiter.SubscriberId);
                subscriberNotes.RecruiterId = rec.RecruiterId;

                BaseModelFactory.SetDefaultsForAddNew(subscriberNotes);
                await _repository.SubscriberNotesRepository.AddNotes(subscriberNotes);
            }
            else if (subscriberNotesDto.SubscriberNotesGuid != Guid.Empty)
            {
                var subscriberNote = await _repository.SubscriberNotesRepository.GetSubscriberNotesBySubscriberNotesGuid(subscriberNotesDto.SubscriberNotesGuid ?? Guid.Empty);
                subscriberNote.Notes = subscriberNotesDto.Notes;
                subscriberNote.ViewableByOthersInRecruiterCompany = subscriberNotesDto.ViewableByOthersInRecruiterCompany;
                subscriberNote.ModifyDate = DateTime.Now;
                await _repository.SubscriberNotesRepository.UpdateNotes(subscriberNote);
            }

        }

        public async Task<List<SubscriberNotesDto>> GetSubscriberNotesBySubscriberGuid(string subscriberGuid, string recruiterGuid, string searchquery)
        {
            List<SubscriberNotesDto> subscriberNotesDtoList = new List<SubscriberNotesDto>();
            //subscriberGuid
            Guid sGuid = Guid.Parse(subscriberGuid);
            Guid rGuid = Guid.Parse(recruiterGuid);

            //get subscriber record for candidate
            var subscriberData = await _repository.SubscriberRepository.GetSubscriberByGuidAsync(sGuid);

            //get recruiter record
            //get recruiter by subscriberGuid  map to recruiter and get recruiterId
            //recruiter is also a subscriber
            var recruiterData = await _repository.Subscriber.GetSubscriberByGuidAsync(rGuid);
            var rec = await _repository.RecruiterRepository.GetRecruiterBySubscriberId(recruiterData.SubscriberId);

            List<SubscriberNotesDto> recruiterPrivateNotes;
            List<SubscriberNotesDto> subscriberPublicNotes;

            //get notes for subscriber that are private and visible to current logged in recruiter
            var recruiterPrivateNotesQueryable = from subscriberNote in await _repository.SubscriberNotesRepository.GetAllAsync()
                                                 join recruiter in await _repository.RecruiterRepository.GetAllAsync() on subscriberNote.RecruiterId equals recruiter.RecruiterId
                                                 join subscriber in await _repository.SubscriberRepository.GetAllAsync() on recruiter.SubscriberId equals subscriber.SubscriberId
                                                 where subscriberNote.SubscriberId.Equals(subscriberData.SubscriberId) && subscriberNote.IsDeleted.Equals(0) && subscriberNote.RecruiterId.Equals(rec.RecruiterId) && subscriberNote.ViewableByOthersInRecruiterCompany.Equals(false)
                                                 select new SubscriberNotesDto()
                                                 {
                                                     ViewableByOthersInRecruiterCompany = subscriberNote.ViewableByOthersInRecruiterCompany,
                                                     Notes = subscriberNote.Notes,
                                                     SubscriberNotesGuid = subscriberNote.SubscriberNotesGuid,
                                                     RecruiterGuid = (Guid)subscriber.SubscriberGuid,
                                                     SubscriberGuid = (Guid)subscriberData.SubscriberGuid,
                                                     CreateDate = subscriberNote.CreateDate,
                                                     ModifiedDate = (DateTime)subscriberNote.ModifyDate,
                                                     RecruiterName = recruiter.FirstName + " " + recruiter.LastName
                                                 };



            //get notes for subscriber that are public and visible to recruiters of current logged in recruiter company
            var subscriberPublicNotesQueryable = from subscriberNote in await _repository.SubscriberNotesRepository.GetAllAsync()
                                                 join recruiter in await _repository.RecruiterRepository.GetAllAsync() on subscriberNote.RecruiterId equals recruiter.RecruiterId
                                                 join company in await _repository.Company.GetAllCompanies() on recruiter.CompanyId equals company.CompanyId
                                                 join subscriber in await _repository.SubscriberRepository.GetAllAsync() on recruiter.SubscriberId equals subscriber.SubscriberId
                                                 where subscriberNote.SubscriberId.Equals(subscriberData.SubscriberId) && subscriberNote.IsDeleted.Equals(0) && recruiter.CompanyId.Equals(rec.CompanyId) && subscriberNote.ViewableByOthersInRecruiterCompany.Equals(true)
                                                 select new SubscriberNotesDto()
                                                 {
                                                     ViewableByOthersInRecruiterCompany = subscriberNote.ViewableByOthersInRecruiterCompany,
                                                     Notes = subscriberNote.Notes,
                                                     SubscriberNotesGuid = subscriberNote.SubscriberNotesGuid,
                                                     RecruiterGuid = (Guid)subscriber.SubscriberGuid,
                                                     SubscriberGuid = (Guid)subscriberData.SubscriberGuid,
                                                     CreateDate = subscriberNote.CreateDate,
                                                     ModifiedDate = (DateTime)subscriberNote.ModifyDate,
                                                     RecruiterName = recruiter.FirstName + " " + recruiter.LastName
                                                 };

            if (!string.IsNullOrEmpty(searchquery))
            {
                recruiterPrivateNotes = await recruiterPrivateNotesQueryable.Where(pn => pn.CreateDate.Date == DateTime.Parse(searchquery).Date).ToListAsync();
                subscriberPublicNotes = await subscriberPublicNotesQueryable.Where(pn => pn.CreateDate.Date == DateTime.Parse(searchquery).Date).ToListAsync();
            }
            else
            {
                recruiterPrivateNotes = await recruiterPrivateNotesQueryable.ToListAsync();
                subscriberPublicNotes = await subscriberPublicNotesQueryable.ToListAsync();
            }


            subscriberNotesDtoList.AddRange(recruiterPrivateNotes);
            subscriberNotesDtoList.AddRange(subscriberPublicNotes);

            return subscriberNotesDtoList.OrderByDescending(sn => sn.CreateDate).ToList();
        }

        public async Task<bool> DeleteSubscriberNote(Guid subscriberNotesGuid)
        {
            bool isDeleted = false;
            //check if notes exist for subscriber
            var subscriberNote = await _repository.SubscriberNotesRepository.GetSubscriberNotesBySubscriberNotesGuid(subscriberNotesGuid);
            if (subscriberNote != null)
            {
                subscriberNote.IsDeleted = 1;
                await _repository.SubscriberNotesRepository.UpdateNotes(subscriberNote);
                isDeleted = true;
            }

            return isDeleted;
        }
        #endregion



        #region resume parsing 

        public async Task<bool> ImportResume(ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requiresMerge = false;
                // Get the subscriber 
                //                Subscriber subscriber = SubscriberFactory.GetSubscriberById(_db, resumeParse.SubscriberId);
                Subscriber subscriber = await _repository.Subscriber.GetSubscriberByIdAsync(resumeParse.SubscriberId);

                if (subscriber == null)
                {
                    return false;
                }
                // Import Contact Info 
                if (await _ImportResumeContactInfo(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import skills 
                if (await _ImportResumeSkills(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import work history  
                if (await _ImportResumeWorkHistory(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;
                // Import education history  
                if (await _ImportResumeEducationHistory(subscriber, resumeParse, resume) == true)
                    requiresMerge = true;

                return requiresMerge;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        private async Task<bool> _ImportResumeEducationHistory(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requiresMerge = false;
                List<SubscriberEducationHistoryDto> parsedEducationHistory = Utils.ParseEducationHistoryFromHrXml(resume);
                IList<SubscriberEducationHistory> educationHistory = SubscriberFactory.GetSubscriberEducationHistoryById(_db, subscriber.SubscriberId);
                foreach (SubscriberEducationHistoryDto eh in parsedEducationHistory)
                {
                    string parsedInstitutionName = eh.EducationalInstitution.ToLower();
                    string parsedEducationalDegree = eh.EducationalDegree.ToLower();
                    string parsedEducationalDegreeType = eh.EducationalDegreeType.ToLower();
                    var ExistingInstitution = educationHistory.Where(s => s.EducationalInstitution.Name.ToLower() == parsedInstitutionName).FirstOrDefault();
                    // get or create the company specified by the work history 
                    EducationalInstitution institution = await EducationalInstitutionFactory.GetOrAdd(_db, parsedInstitutionName);
                    EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_db, parsedEducationalDegree);
                    // Do not allow user defined degree types so call GetOrDefault 
                    EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetOrDefault(_db, parsedEducationalDegreeType);
                    // if its not an existing college just add it

                    if (ExistingInstitution == null)
                    {

                        SubscriberEducationHistory newEducationHistory = await SubscriberEducationHistoryFactory.AddEducationHistoryForSubscriber(_db, subscriber, eh, institution, educationalDegree, educationalDegreeType);
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "Object", string.Empty, string.Empty, (int)ResumeParseStatus.Merged, newEducationHistory.SubscriberEducationHistoryGuid);
                    }
                    else // Check to see its an update to an existing work history 
                    {
                        // todo - at some point make this more intelligent, edge cases as two degrees from the same institution may not be handled very well
                        // well
                        var ExistingInstitutions = educationHistory.Where(s => s.EducationalInstitution.Name.ToLower() == parsedInstitutionName).ToList();
                        foreach (SubscriberEducationHistory seh in ExistingInstitutions)
                            if (await MergeEducationalHistories(resumeParse, seh, eh) == true)
                                requiresMerge = true;
                    }

                }
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requiresMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }

        }

        private async Task<bool> MergeEducationalHistories(ResumeParse resumeParse, SubscriberEducationHistory educationHistory, SubscriberEducationHistoryDto parsedEducationHistory)
        {
            bool requiresMerge = false;
            if (parsedEducationHistory.StartDate != null && parsedEducationHistory.StartDate != DateTime.MinValue)
            {
                if (educationHistory.StartDate == null || educationHistory.StartDate == DateTime.MinValue || educationHistory.StartDate == parsedEducationHistory.StartDate)
                {
                    educationHistory.StartDate = parsedEducationHistory.StartDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "StartDate", educationHistory.StartDate.Value.ToString("o"), parsedEducationHistory.StartDate.Value.ToString("o"), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you start studying at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory", "StartDate", educationHistory.StartDate.Value.ToString("o"), parsedEducationHistory.StartDate.Value.ToString("o"), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }


            if (parsedEducationHistory.EndDate != null && parsedEducationHistory.EndDate != DateTime.MinValue)
            {
                if (educationHistory.EndDate == null || educationHistory.EndDate == DateTime.MinValue || educationHistory.EndDate == parsedEducationHistory.EndDate)
                {
                    educationHistory.EndDate = parsedEducationHistory.EndDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "EndDate", educationHistory.EndDate.Value.ToShortDateString(), parsedEducationHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you complete your studies at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory", "EndDate", educationHistory.EndDate.Value.ToShortDateString(), parsedEducationHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }


            if (parsedEducationHistory.DegreeDate != null && parsedEducationHistory.DegreeDate != DateTime.MinValue)
            {
                if (educationHistory.DegreeDate == null || educationHistory.DegreeDate == DateTime.MinValue || educationHistory.DegreeDate == parsedEducationHistory.DegreeDate)
                {
                    educationHistory.DegreeDate = parsedEducationHistory.DegreeDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory", "DegreeDate", educationHistory.DegreeDate.Value.ToShortDateString(), parsedEducationHistory.DegreeDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"When did you complete your degree at {educationHistory.EducationalInstitution.Name}?", "SubscriberWorkHistory", "DegreeDate", educationHistory.DegreeDate.Value.ToShortDateString(), parsedEducationHistory.DegreeDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }
            if (string.IsNullOrWhiteSpace(parsedEducationHistory.EducationalDegreeType) == false)
            {
                string degreeType = Utils.RemoveNewlines(educationHistory.EducationalDegreeType.DegreeType.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedDegreeType = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedEducationHistory.EducationalDegreeType.Trim()));

                if (string.IsNullOrWhiteSpace(degreeType) || degreeType.ToLower() == parsedDegreeType.ToLower())
                {
                    // case where current value is not specified but parsed value is 
                    if (string.IsNullOrWhiteSpace(degreeType) == true && string.IsNullOrWhiteSpace(parsedDegreeType) == false)
                    {
                        EducationalDegreeType newDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_db, parsedDegreeType);
                        educationHistory.EducationalDegreeTypeId = newDegreeType.EducationalDegreeTypeId;
                    }
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory.EducationalDegreeTypeId", "EducationalDegreeTypeId", degreeType, parsedDegreeType, (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    EducationalDegreeType educationalDegreeType = EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(_db, parsedDegreeType);
                    // only add educational degree type if the parsed value is in the CC list of degree types since
                    // degree types is a lookup list curated by CC and user's cannot add new value to it
                    if (educationalDegreeType != null)
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"What type of degree did you earn at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory.EducationalDegreeTypeId", "EducationalDegreeTypeId", degreeType, parsedDegreeType, (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                        requiresMerge = true;

                    }
                }

            }

            if (string.IsNullOrWhiteSpace(parsedEducationHistory.EducationalDegree) == false)
            {
                string degree = Utils.RemoveNewlines(educationHistory.EducationalDegree.Degree.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedDegree = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedEducationHistory.EducationalDegree.Trim()));

                if (string.IsNullOrWhiteSpace(degree) || degree.ToLower() == parsedDegree.ToLower())
                {
                    // case where current value is not specified but parsed value is 
                    if (string.IsNullOrWhiteSpace(degree) == true && string.IsNullOrWhiteSpace(parsedDegree) == false)
                    {
                        EducationalDegree newDegreeType = await EducationalDegreeFactory.GetOrAdd(_db, parsedDegree);
                        educationHistory.EducationalDegreeId = newDegreeType.EducationalDegreeId;
                    }
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, string.Empty, "SubscriberEducationHistory.EducationalDegreeId", "EducationalDegreeId", degree, parsedDegree, (int)ResumeParseStatus.Merged, educationHistory.SubscriberEducationHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.EducationHistory, $"What was the major of your degree earned at {educationHistory.EducationalInstitution.Name}?", "SubscriberEducationHistory.EducationalDegreeId", "EducationalDegreeId", degree, parsedDegree, (int)ResumeParseStatus.MergeNeeded, educationHistory.SubscriberEducationHistoryGuid);
                    requiresMerge = true;
                }

            }

            await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
            return requiresMerge;
        }






        /// <summary>
        ///  enhances and notes differences in user's work history on record vs their work history parsed from their resume
        /// </summary>
        /// <param name="resumeParse"></param>
        /// <param name="workHistory"></param>
        /// <param name="parsedWorkHistory"></param>
        /// <returns></returns>
        private async Task<bool> MergeWorkHistories(ResumeParse resumeParse, SubscriberWorkHistory workHistory, SubscriberWorkHistoryDto parsedWorkHistory)
        {
            bool requireMerge = false;
            if (parsedWorkHistory.StartDate != null && parsedWorkHistory.StartDate != DateTime.MinValue)
            {
                if (workHistory.StartDate == null || workHistory.StartDate == DateTime.MinValue || workHistory.StartDate == parsedWorkHistory.StartDate)
                {
                    workHistory.StartDate = parsedWorkHistory.StartDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "StartDate", workHistory.StartDate.Value.ToShortDateString(), parsedWorkHistory.StartDate.Value.ToString(), (int)ResumeParseStatus.Merged, workHistory.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"When did you start employment at {workHistory.Company.CompanyName}?", "SubscriberWorkHistory", "StartDate", workHistory.StartDate.Value.ToShortDateString(), parsedWorkHistory.StartDate.Value.ToShortDateString(), (int)ResumeParseStatus.MergeNeeded, workHistory.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }

            if (parsedWorkHistory.EndDate != null && parsedWorkHistory.EndDate != DateTime.MinValue)
            {
                if (workHistory.EndDate == null || workHistory.EndDate == DateTime.MinValue || workHistory.EndDate == parsedWorkHistory.EndDate)
                {
                    workHistory.EndDate = parsedWorkHistory.EndDate;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "EndDate", workHistory.EndDate.Value.ToShortDateString(), parsedWorkHistory.EndDate.Value.ToShortDateString(), (int)ResumeParseStatus.Merged, workHistory.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"When did your employment at {workHistory.Company.CompanyName} end?", "SubscriberWorkHistory", "EndDate", workHistory.EndDate.Value.ToString("o"), parsedWorkHistory.EndDate.Value.ToString("o"), (int)ResumeParseStatus.MergeNeeded, workHistory.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }

            if (string.IsNullOrWhiteSpace(parsedWorkHistory.Title) == false)
            {
                string jobTitle = Utils.RemoveNewlines(workHistory.Title.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedJobTitle = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedWorkHistory.Title.Trim()));


                if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.ToLower() == parsedJobTitle.ToLower())
                {
                    // html encode to be consistent with api endpoints that encode to protect again script injection 
                    workHistory.Title = parsedJobTitle;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "Title", jobTitle, parsedJobTitle, (int)ResumeParseStatus.Merged, workHistory.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"While working at {workHistory.Company.CompanyName}, what was your job title?", "SubscriberWorkHistory", "Title", jobTitle, parsedJobTitle, (int)ResumeParseStatus.MergeNeeded, workHistory.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }


            if (string.IsNullOrWhiteSpace(parsedWorkHistory.JobDescription) == false)
            {
                string jobDescription = Utils.RemoveNewlines(workHistory.JobDescription.Trim());
                // html encode to be consistent with api endpoints that encode to protect again script injection 
                string parsedJobDescription = Utils.RemoveNewlines(HttpUtility.HtmlEncode(parsedWorkHistory.JobDescription.Trim()));

                if (string.IsNullOrWhiteSpace(jobDescription) || jobDescription.ToLower() == parsedJobDescription.ToLower())
                {
                    workHistory.JobDescription = parsedJobDescription;
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "JobDescription", jobDescription, parsedJobDescription, (int)ResumeParseStatus.Merged, workHistory.SubscriberWorkHistoryGuid);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, $"While working at {workHistory.Company.CompanyName}, what was your job description?", "SubscriberWorkHistory", "JobDescription", jobDescription, parsedJobDescription, (int)ResumeParseStatus.MergeNeeded, workHistory.SubscriberWorkHistoryGuid);
                    requireMerge = true;
                }

            }


            await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
            return requireMerge;
        }

        /// <summary>
        /// import work history information found in user's resume 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>
        private async Task<bool> _ImportResumeWorkHistory(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            bool requireMerge = false;
            try
            {
                List<SubscriberWorkHistoryDto> parsedWorkHistory = Utils.ParseWorkHistoryFromHrXml(resume);
                IList<SubscriberWorkHistory> workHistory = SubscriberFactory.GetSubscriberWorkHistoryById(_db, subscriber.SubscriberId);
                foreach (SubscriberWorkHistoryDto wh in parsedWorkHistory)
                {
                    string parsedCompanyName = wh.Company.ToLower();
                    var ExistingCompany = workHistory.Where(s => s.Company.CompanyName.ToLower() == parsedCompanyName).FirstOrDefault();
                    // get or create the company specified by the work history 
                    Company company = await CompanyFactory.GetOrAdd(_db, parsedCompanyName);
                    // if its not an existing company just add it
                    if (ExistingCompany == null)
                    {
                        SubscriberWorkHistory newWorkHistory = await SubscriberWorkHistoryFactory.AddWorkHistoryForSubscriber(_db, subscriber, wh, company);
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.WorkHistory, string.Empty, "SubscriberWorkHistory", "Object", string.Empty, string.Empty, (int)ResumeParseStatus.Merged, newWorkHistory.SubscriberWorkHistoryGuid);
                    }
                    else // Check to see its an update to an existing work history 
                    {
                        // todo - at some point make this more intelligent, edge cases as two stints at the same company may not be handled very
                        // well
                        var ExistingCompanies = workHistory.Where(s => s.Company.CompanyName.ToLower() == parsedCompanyName).ToList();
                        foreach (SubscriberWorkHistory swh in ExistingCompanies)
                            if (await MergeWorkHistories(resumeParse, swh, wh) == true)
                                requireMerge = true;
                    }
                }
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeWorkHistory threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }

        }


        /// <summary>
        /// Import skills found in the resume 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>

        private async Task<bool> _ImportResumeSkills(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            try
            {
                bool requireMerge = false;
                List<string> parsedSkills = Utils.ParseSkillsFromHrXML(resume);
                IList<SubscriberSkill> skills = SubscriberFactory.GetSubscriberSkillsById(_db, subscriber.SubscriberId);
                HashSet<string> foundSkills = new HashSet<string>();
                foreach (string skillName in parsedSkills)
                {


                    string parsedSkill = skillName.ToLower();
                    // were getting back duplicate skills from sovren, so we'll ignore the dupes
                    if (foundSkills.Contains(parsedSkill) == false)
                    {
                        foundSkills.Add(parsedSkill);
                        int status = (int)ResumeParseStatus.MergeNeeded;
                        string prompt = "Do you have the following skill?";
                        var existingSkill = skills.Where(s => s.Skill.SkillName.ToLower() == parsedSkill).FirstOrDefault();
                        if (existingSkill != null)
                        {
                            prompt = string.Empty;
                            status = (int)ResumeParseStatus.Duplicate;
                        }
                        else
                            requireMerge = true;

                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.Skills, prompt, "SubscriberSkill", "SkillName", parsedSkill, parsedSkill, status, subscriber.SubscriberGuid.Value);
                    }

                }
                // save resume parse results 
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();
                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeSkills threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }
        }





        /// <summary>
        /// Import resume contact information 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="resumeParse"></param>
        /// <param name="resume"></param>
        /// <returns></returns>

        private async Task<bool> _ImportResumeContactInfo(Subscriber subscriber, ResumeParse resumeParse, string resume)
        {
            bool requireMerge = false;
            try
            {

                SubscriberContactInfoDto contactInfo = Utils.ParseContactInfoFromHrXML(resume);

                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.FirstName) || subscriber.FirstName.Trim() == contactInfo.FirstName.Trim())
                {
                    subscriber.FirstName = contactInfo.FirstName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which first name do you prefer?", "Subscriber", "FirstName", subscriber.FirstName, contactInfo.FirstName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.LastName) || subscriber.LastName.Trim() == contactInfo.LastName.Trim())
                {
                    subscriber.LastName = contactInfo.LastName.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which last name do you prefer?", "Subscriber", "LastName", subscriber.LastName, contactInfo.LastName, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }


                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.City) || subscriber.City.Trim() == contactInfo.City.Trim())
                {
                    subscriber.City = contactInfo.City.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "City", subscriber.City, contactInfo.City, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "What city do you currently reside in?", "Subscriber", "City", subscriber.City, contactInfo.City, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }


                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.PostalCode) || subscriber.PostalCode.Trim() == contactInfo.PostalCode.Trim())
                {
                    subscriber.PostalCode = contactInfo.PostalCode.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "PostalCode", subscriber.PostalCode, contactInfo.PostalCode, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "What is your current postal code?", "Subscriber", "PostalCode", subscriber.PostalCode, contactInfo.PostalCode, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                // case of existing property is empty or existing property and parsed property are equal 
                if (string.IsNullOrWhiteSpace(subscriber.Address) || subscriber.Address.Trim() == contactInfo.Address.Trim())
                {
                    subscriber.Address = contactInfo.Address.Trim();
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "Address", subscriber.Address, contactInfo.Address, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                }
                else
                {
                    await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which address is correct?", "Subscriber", "Address", subscriber.Address, contactInfo.Address, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                    requireMerge = true;
                }



                State state = StateFactory.GetStateByStateCode(_db, contactInfo.State);
                if (state != null)
                {
                    if (subscriber.StateId <= 0 || subscriber.State == null || state.StateId == subscriber.StateId)
                    {
                        subscriber.StateId = state.StateId;
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber.StateCode", "StateCode", subscriber.State.Code, contactInfo.State, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                    }
                    else
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "In which state/province do you currently reside?", "Subscriber.StateCode", "StateCode", subscriber.State.Code, contactInfo.State, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                        requireMerge = true;
                    }

                }

                // remove extraneous characters from parsed phone number 
                contactInfo.PhoneNumber = contactInfo.PhoneNumber.Trim().Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                Regex phoneRegex = new Regex(@"^([0-9]{0,3})?[2-9]{1}[0-9]{9}$");
                // validate that the parsed phone number is valid 
                if (phoneRegex.IsMatch(contactInfo.PhoneNumber))
                {
                    if (string.IsNullOrWhiteSpace(subscriber.PhoneNumber) || subscriber.PhoneNumber.Trim() == contactInfo.PhoneNumber)
                    {
                        subscriber.PhoneNumber = contactInfo.PhoneNumber.Trim();
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, string.Empty, "Subscriber", "PhoneNumber", subscriber.PhoneNumber, contactInfo.PhoneNumber, (int)ResumeParseStatus.Merged, subscriber.SubscriberGuid.Value);
                    }
                    else
                    {
                        await _repository.ResumeParseResultRepository.CreateResumeParseResultAsync(resumeParse.ResumeParseId, (int)ResumeParseSection.ContactInfo, "Which phone number do you prefer?", "Subscriber", "PhoneNumber", subscriber.PhoneNumber, contactInfo.PhoneNumber, (int)ResumeParseStatus.MergeNeeded, subscriber.SubscriberGuid.Value);
                        requireMerge = true;
                    }


                }

                // save the subscriber
                await _repository.Subscriber.SaveAsync();
                // save resume parse results 
                await _repository.ResumeParseResultRepository.SaveResumeParseResultAsync();

                return requireMerge;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService:_ImportResumeContactInfo threw an exception -> {e.Message} for subscriber {subscriber.SubscriberId} profile data = {resume}");
                return false;
            }
        }


        #endregion

        public async Task<Subscriber> GetSubscriber(ODataQueryOptions<Subscriber> options)
        {
            var queryableSubscriber = options.ApplyTo(await _repository.SubscriberRepository.GetAllSubscribersAsync());
            var subscriberList=await queryableSubscriber.Cast<Subscriber>().Where(s=>s.IsDeleted==0).ToListAsync();

            return subscriberList.Count>0 ? subscriberList[0] :null;
        }



    }



}