using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Domain.Models.G2;
using System.Data.SqlClient;
using System.Data;
using UpDiddyApi.ApplicationCore.Exceptions;


namespace UpDiddyApi.ApplicationCore.Repository
{
    public class ProfileRepository : UpDiddyRepositoryBase<Profile>, IProfileRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public ProfileRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteProfile(Guid profileGuid)
        {
            var profile = _dbContext.Profile.Where(p => p.ProfileGuid == profileGuid && p.IsDeleted == 0).FirstOrDefault();
            if (profile == null)
                throw new NotFoundException("profile not found");
            var pending = _dbContext.AzureIndexStatus.Where(a => a.IsDeleted == 0 && a.Name == "Pending").FirstOrDefault();
            profile.IsDeleted = 1;
            profile.ModifyDate = DateTime.UtcNow;
            profile.ModifyGuid = Guid.Empty;
            profile.AzureIndexStatusId = pending.AzureIndexStatusId;
            _dbContext.Update(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Profile> GetProfileForRecruiter(Guid profileGuid, Guid subscriberGuid)
        {
            return await (from p in _dbContext.Profile
                        .Include(c => c.Company)
                        .Include(ct => ct.ContactType)
                        .Include(c => c.City)
                        .Include(et => et.ProfileEmploymentTypes).ThenInclude(pet => pet.EmploymentType)
                        .Include(el => el.ExperienceLevel)
                        .Include(s => s.Subscriber)
                        .Include(s => s.State).ThenInclude(c => c.Country)
                        .Include(p => p.Postal)
                          join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                          join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                          join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                          join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                          where p.ProfileGuid == profileGuid && s.SubscriberGuid == subscriberGuid
                          select p)
                    .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateProfile(ProfileDto profileDto)
        {
            var companyGuid = new SqlParameter { ParameterName = "@CompanyGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.CompanyGuid ?? DBNull.Value };
            var subscriberGuid = new SqlParameter { ParameterName = "@SubscriberGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.SubscriberGuid ?? DBNull.Value };
            var cityGuid = new SqlParameter { ParameterName = "@CityGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.CityGuid ?? DBNull.Value };
            var stateGuid = new SqlParameter { ParameterName = "@StateGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.StateGuid ?? DBNull.Value };
            var postalGuid = new SqlParameter { ParameterName = "@PostalGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.PostalGuid ?? DBNull.Value };
            var experienceLevelGuid = new SqlParameter { ParameterName = "@ExperienceLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.ExperienceLevelGuid ?? DBNull.Value };
            DataTable employmentTypeTable = new DataTable();
            employmentTypeTable.Columns.Add("Guid", typeof(Guid));
            if (profileDto.EmploymentTypeGuids != null && profileDto.EmploymentTypeGuids.Count > 0)
            {
                foreach (var employmentTypeGuid in profileDto.EmploymentTypeGuids)
                {
                    employmentTypeTable.Rows.Add(employmentTypeGuid);
                }
            }
            var employmentTypeGuids = new SqlParameter("@EmploymentTypeGuids", employmentTypeTable);
            employmentTypeGuids.SqlDbType = SqlDbType.Structured;
            employmentTypeGuids.TypeName = "dbo.GuidList";
            var contactTypeGuid = new SqlParameter { ParameterName = "@ContactTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.ContactTypeGuid ?? DBNull.Value };
            var firstName = new SqlParameter { ParameterName = "@FirstName", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.FirstName ?? DBNull.Value };
            var lastName = new SqlParameter { ParameterName = "@LastName", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.LastName ?? DBNull.Value };
            var email = new SqlParameter { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Email ?? DBNull.Value };
            var phoneNumber = new SqlParameter { ParameterName = "@PhoneNumber", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.PhoneNumber ?? DBNull.Value };
            var streetAddress = new SqlParameter { ParameterName = "@StreetAddress", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.StreetAddress ?? DBNull.Value };
            var title = new SqlParameter { ParameterName = "@Title", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Title ?? DBNull.Value };
            var isWillingToTravel = new SqlParameter { ParameterName = "@IsWillingToTravel", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsWillingToTravel ?? false };
            var isActiveJobSeeker = new SqlParameter { ParameterName = "@IsActiveJobSeeker", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsActiveJobSeeker ?? false };
            var isCurrentlyEmployed = new SqlParameter { ParameterName = "@IsCurrentlyEmployed", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsCurrentlyEmployed ?? false };
            var isWillingToWorkProBono = new SqlParameter { ParameterName = "@IsWillingToWorkProBono", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsWillingToWorkProBono ?? false };
            var currentRate = new SqlParameter { ParameterName = "@CurrentRate", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)profileDto.CurrentRate ?? 0 };
            var desiredRate = new SqlParameter { ParameterName = "@DesiredRate", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)profileDto.DesiredRate ?? 0 };
            var goals = new SqlParameter { ParameterName = "@Goals", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Goals ?? DBNull.Value };
            var preferences = new SqlParameter { ParameterName = "@Preferences", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Preferences ?? DBNull.Value };
            var skillsNote = new SqlParameter { ParameterName = "@SkillsNote", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.SkillsNote ?? DBNull.Value };
            var profileGuid = new SqlParameter { ParameterName = "@ProfileGuid", SqlDbType = SqlDbType.UniqueIdentifier, Size = -1, Direction = ParameterDirection.Output };
            var validationErrors = new SqlParameter { ParameterName = "@ValidationErrors", SqlDbType = SqlDbType.NVarChar, Size = -1, Direction = ParameterDirection.Output };
            var spParams = new object[] { companyGuid, subscriberGuid, cityGuid, stateGuid, postalGuid, experienceLevelGuid, contactTypeGuid, firstName, lastName, email, phoneNumber, streetAddress, title, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono, currentRate, desiredRate, goals, preferences, skillsNote, employmentTypeGuids, profileGuid, validationErrors };

            try
            {
                var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [G2].[System_Create_Profile] @CompanyGuid, @SubscriberGuid, @CityGuid, @StateGuid, @PostalGuid, @ExperienceLevelGuid, @ContactTypeGuid, @FirstName, @LastName, @Email, @PhoneNumber, @StreetAddress, @Title, @IsWillingToTravel, @IsActiveJobSeeker, @IsCurrentlyEmployed, @IsWillingToWorkProBono, @CurrentRate, @DesiredRate, @Goals, @Preferences, @SkillsNote, @EmploymentTypeGuids, @ProfileGuid OUTPUT, @ValidationErrors OUTPUT", spParams);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, $"An error occurred in ProfileRepository.CreateProfile: {e.Message}");
                throw new FailedValidationException("An unknown error occurred; see logging for details.");
            }

            if (!string.IsNullOrWhiteSpace(validationErrors.Value.ToString()))
                throw new FailedValidationException(validationErrors.Value.ToString());

            return Guid.Parse(profileGuid.Value.ToString());
        }

        public async Task UpdateProfileForRecruiter(ProfileDto profileDto, Guid subscriber)
        {
            var recruiterSubscriberGuid = new SqlParameter { ParameterName = "@RecruiterSubscriberGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)subscriber ?? DBNull.Value };
            var profileGuid = new SqlParameter { ParameterName = "@ProfileGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.ProfileGuid ?? DBNull.Value };
            var companyGuid = new SqlParameter { ParameterName = "@CompanyGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.CompanyGuid ?? DBNull.Value };
            var subscriberGuid = new SqlParameter { ParameterName = "@SubscriberGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.SubscriberGuid ?? DBNull.Value };
            var cityGuid = new SqlParameter { ParameterName = "@CityGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.CityGuid ?? DBNull.Value };
            var stateGuid = new SqlParameter { ParameterName = "@StateGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.StateGuid ?? DBNull.Value };
            var postalGuid = new SqlParameter { ParameterName = "@PostalGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.PostalGuid ?? DBNull.Value };
            var experienceLevelGuid = new SqlParameter { ParameterName = "@ExperienceLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.ExperienceLevelGuid ?? DBNull.Value };
            DataTable employmentTypeTable = new DataTable();
            employmentTypeTable.Columns.Add("Guid", typeof(Guid));
            if (profileDto.EmploymentTypeGuids != null && profileDto.EmploymentTypeGuids.Count > 0)
            {
                foreach (var employmentTypeGuid in profileDto.EmploymentTypeGuids)
                {
                    employmentTypeTable.Rows.Add(employmentTypeGuid);
                }
            }
            var employmentTypeGuids = new SqlParameter("@EmploymentTypeGuids", employmentTypeTable);
            employmentTypeGuids.SqlDbType = SqlDbType.Structured;
            employmentTypeGuids.TypeName = "dbo.GuidList";
            var contactTypeGuid = new SqlParameter { ParameterName = "@ContactTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileDto.ContactTypeGuid ?? DBNull.Value };
            var firstName = new SqlParameter { ParameterName = "@FirstName", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.FirstName ?? DBNull.Value };
            var lastName = new SqlParameter { ParameterName = "@LastName", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.LastName ?? DBNull.Value };
            var email = new SqlParameter { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Email ?? DBNull.Value };
            var phoneNumber = new SqlParameter { ParameterName = "@PhoneNumber", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.PhoneNumber ?? DBNull.Value };
            var streetAddress = new SqlParameter { ParameterName = "@StreetAddress", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.StreetAddress ?? DBNull.Value };
            var title = new SqlParameter { ParameterName = "@Title", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Title ?? DBNull.Value };
            var isWillingToTravel = new SqlParameter { ParameterName = "@IsWillingToTravel", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsWillingToTravel ?? false };
            var isActiveJobSeeker = new SqlParameter { ParameterName = "@IsActiveJobSeeker", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsActiveJobSeeker ?? false };
            var isCurrentlyEmployed = new SqlParameter { ParameterName = "@IsCurrentlyEmployed", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsCurrentlyEmployed ?? false };
            var isWillingToWorkProBono = new SqlParameter { ParameterName = "@IsWillingToWorkProBono", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)profileDto.IsWillingToWorkProBono ?? false };
            var currentRate = new SqlParameter { ParameterName = "@CurrentRate", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)profileDto.CurrentRate ?? 0 };
            var desiredRate = new SqlParameter { ParameterName = "@DesiredRate", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)profileDto.DesiredRate ?? 0 };
            var goals = new SqlParameter { ParameterName = "@Goals", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Goals ?? DBNull.Value };
            var preferences = new SqlParameter { ParameterName = "@Preferences", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.Preferences ?? DBNull.Value };
            var skillsNote = new SqlParameter { ParameterName = "@SkillsNote", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)profileDto.SkillsNote ?? DBNull.Value };
            var validationErrors = new SqlParameter { ParameterName = "@ValidationErrors", SqlDbType = SqlDbType.NVarChar, Size = -1, Direction = ParameterDirection.Output };
            var spParams = new object[] { recruiterSubscriberGuid, profileGuid, companyGuid, subscriberGuid, cityGuid, stateGuid, postalGuid, experienceLevelGuid, contactTypeGuid, firstName, lastName, email, phoneNumber, streetAddress, title, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono, currentRate, desiredRate, goals, preferences, skillsNote, employmentTypeGuids, validationErrors };

            try
            {
                var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [G2].[System_Update_Profile] @RecruiterSubscriberGuid, @ProfileGuid, @CompanyGuid, @SubscriberGuid, @CityGuid, @StateGuid, @PostalGuid, @ExperienceLevelGuid, @ContactTypeGuid, @FirstName, @LastName, @Email, @PhoneNumber, @StreetAddress, @Title, @IsWillingToTravel, @IsActiveJobSeeker, @IsCurrentlyEmployed, @IsWillingToWorkProBono, @CurrentRate, @DesiredRate, @Goals, @Preferences, @SkillsNote, @EmploymentTypeGuids, @ValidationErrors OUTPUT", spParams);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, $"An error occurred in ProfileRepository.UpdateProfileForRecruiter: {e.Message}");
                throw new FailedValidationException("An unknown error occurred; see logging for details.");
            }

            if (!string.IsNullOrWhiteSpace(validationErrors.Value.ToString()))
                throw new FailedValidationException(validationErrors.Value.ToString());
        }

        public async Task UpdateAzureIndexStatus(Guid profileGuid, Guid azureIndexStatusGuid, string azureSearchIndexInfo)
        {
            var profile = _dbContext.Profile.Where(p => p.ProfileGuid == profileGuid).FirstOrDefault();
            var azureIndexStatus = _dbContext.AzureIndexStatus.Where(a => a.AzureIndexStatusGuid == azureIndexStatusGuid).FirstOrDefault();
            profile.AzureIndexStatusId = azureIndexStatus.AzureIndexStatusId;
            profile.AzureSearchIndexInfo = azureSearchIndexInfo;
            profile.ModifyDate = DateTime.UtcNow;
            profile.ModifyGuid = Guid.Empty;
            _dbContext.Update(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Profile>> GetProfilesByGuidList(List<Guid> profilesGuids)
        {
            List<Profile> rval = await (from p in _dbContext.Profile
                                         .Include(c => c.Company)
                                         .Include(ct => ct.ContactType)
                                         .Include(c => c.City)
                                         .Include(el => el.ExperienceLevel)
                                         .Include(s => s.State)
                                         .Include(s => s.Subscriber
                                       )
                                        where (profilesGuids.Contains(p.ProfileGuid))
                                        select p)
                                       .ToListAsync();
            return rval;
        }


    }



}
