using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.B2B;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberEducationHistoryRepository : UpDiddyRepositoryBase<SubscriberEducationHistory>, ISubscriberEducationHistoryRepository
    {

        private readonly UpDiddyDbContext _dbContext;

        public SubscriberEducationHistoryRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SubscriberEducationHistory>> GetEducationalHistoryBySubscriberGuid(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var educationalHistory = _dbContext.SubscriberEducationHistory
                                        .Where(seh => seh.Subscriber.SubscriberGuid == subscriberGuid &&
                                                      seh.Subscriber.IsDeleted == 0 &&
                                                      seh.IsDeleted == 0)
                                        .Include(seh => seh.EducationalDegree)
                                        .Include(seh => seh.EducationalInstitution)
                                        .Include(seh => seh.EducationalDegreeType)
                                        .Include(seh => seh.EducationalDegreeType.EducationalDegreeTypeCategory)
                                        .Skip(limit * offset)
                                        .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalHistory = educationalHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalHistory = educationalHistory.OrderByDescending(s => s.CreateDate);
                        break;
                    default:
                        educationalHistory = educationalHistory.OrderByDescending(s => s.ModifyDate);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "modifydate":
                        educationalHistory = educationalHistory.OrderBy(s => s.ModifyDate);
                        break;
                    case "createdate":
                        educationalHistory = educationalHistory.OrderBy(s => s.CreateDate);
                        break;
                    default:
                        educationalHistory = educationalHistory.OrderBy(s => s.ModifyDate);
                        break;
                }
            }


            return await educationalHistory.ToListAsync();

        }


        public async Task UpdateCandidateEducationAndTraining(Guid subscriberGuid, SubscriberEducationAssessmentsDto subscriberEducationAssessmentsDto)
        {
            var subscriber = _dbContext.Subscriber.FirstOrDefault(s => s.SubscriberGuid == subscriberGuid);
            var verifiedEducationalDegreeTypes = _dbContext.EducationalDegreeType.Where(edt => edt.IsDeleted == 0 && edt.IsVerified.HasValue && edt.IsVerified.Value).ToList();
            var trainingType = _dbContext.TrainingType.Where(tt => tt.IsDeleted == 0).ToList();

            //get all including deleted ones
            var subscriberEducationHistory = _dbContext.SubscriberEducationHistory
                                        .Where(seh => seh.Subscriber.SubscriberId == subscriber.SubscriberId &&
                                                      seh.Subscriber.IsDeleted == 0)
                                        .Include(seh => seh.EducationalDegree)
                                        .Include(seh => seh.EducationalInstitution)
                                        .Include(seh => seh.EducationalDegreeType)
                                        .Include(seh => seh.EducationalDegreeType.EducationalDegreeTypeCategory)
                                        .ToList();

            //get all including deleted ones
            var subscriberTrainingHistory = _dbContext.SubscriberTraining
                            .Where(st => st.Subscriber.SubscriberId == subscriber.SubscriberId &&
                                          st.Subscriber.IsDeleted == 0)
                            .Include(st => st.TrainingType)
                            .ToList();

            //list of all incoming valid guids in the request.
            var incomingSubscriberEducationHistoriesGuids = subscriberEducationAssessmentsDto.EducationHistories
                        .Where(eh => eh.EducationHistoryGuid.HasValue && eh.EducationHistoryGuid.Value != Guid.Empty)
                        .Select(eh => (Guid)eh.EducationHistoryGuid).ToList();

            //Perform soft delete
            var subscriberEducationHistoryToDelete = subscriberEducationHistory
                .Where(seh => !incomingSubscriberEducationHistoriesGuids.Contains(seh.SubscriberEducationHistoryGuid))
                .ToList();

            foreach (var educationsToDelete in subscriberEducationHistoryToDelete)
            {
                subscriberEducationHistory.FirstOrDefault(seh => seh.SubscriberEducationHistoryGuid == educationsToDelete.SubscriberEducationHistoryGuid)
                    .IsDeleted = 1;
            }


            if (subscriberEducationAssessmentsDto.EducationHistories != null && subscriberEducationAssessmentsDto.EducationHistories.Count > 0)
            {
                var newSubscriberEducationHistory = new List<SubscriberEducationHistory>();

                foreach (var subscriberEducation in subscriberEducationAssessmentsDto.EducationHistories)
                {
                    var educationalDegree = _dbContext.EducationalDegree.FirstOrDefault(ed => !String.IsNullOrWhiteSpace(subscriberEducation.EducationalDegree) &&
                                            ed.Degree.Trim().Equals(subscriberEducation.EducationalDegree.Trim(), StringComparison.OrdinalIgnoreCase));
                    //if the educationalDegree that was found above is soft-deleted, then unsoft-delete it and use.
                    if (educationalDegree != null && educationalDegree.IsDeleted == 1)
                    {
                        educationalDegree.IsDeleted = 0;
                        educationalDegree.ModifyDate = DateTime.UtcNow;
                    }
                    var educationalInstitution = _dbContext.EducationalInstitution.FirstOrDefault(ed => !String.IsNullOrWhiteSpace(subscriberEducation.Institution) &&
                                                 ed.Name.Trim().Equals(subscriberEducation.Institution.Trim(), StringComparison.OrdinalIgnoreCase));
                    //if the educationalInstitution that was found above is soft-deleted, then unsoft-delete it and use.
                    if (educationalInstitution != null && educationalDegree.IsDeleted == 1)
                    {
                        educationalInstitution.IsDeleted = 0;
                        educationalInstitution.ModifyDate = DateTime.UtcNow;
                    }

                    if (!subscriberEducation.EducationHistoryGuid.HasValue || subscriberEducation.EducationHistoryGuid.Value == Guid.Empty)
                    {

                        _dbContext.SubscriberEducationHistory.Add(new SubscriberEducationHistory
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            SubscriberEducationHistoryGuid = subscriberEducation.EducationHistoryGuid.HasValue && subscriberEducation.EducationHistoryGuid.Value != Guid.Empty ?
                                     subscriberEducation.EducationHistoryGuid.Value : Guid.NewGuid(),
                            IsDeleted = 0,
                            SubscriberId = subscriber.SubscriberId,
                            StartDate = (DateTime?)null,
                            EndDate = (DateTime?)null,
                            DegreeDate = (DateTime?)null,
                            RelevantYear = subscriberEducation.RelevantYear,
                            EducationalDegree = educationalDegree ?? new EducationalDegree
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                EducationalDegreeGuid = Guid.NewGuid(),
                                IsDeleted = 0,
                                Degree = subscriberEducation.EducationalDegree
                            },
                            EducationalInstitution = educationalInstitution ?? new EducationalInstitution
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                EducationalInstitutionGuid = Guid.NewGuid(),
                                IsDeleted = 0,
                                Name = subscriberEducation.Institution
                            },
                            EducationalDegreeType = verifiedEducationalDegreeTypes.FirstOrDefault(edt => edt.EducationalDegreeTypeGuid == subscriberEducation.EducationalDegreeTypeGuid)
                        });

                    }
                    else
                    {
                        var updateEducationHistory = subscriberEducationHistory.FirstOrDefault(seh => seh.SubscriberEducationHistoryGuid == subscriberEducation.EducationHistoryGuid.Value);

                        if(updateEducationHistory != null)
                        {
                            updateEducationHistory.ModifyDate = DateTime.UtcNow;
                            updateEducationHistory.SubscriberEducationHistoryGuid = subscriberEducation.EducationHistoryGuid.HasValue && subscriberEducation.EducationHistoryGuid.Value != Guid.Empty ?
                                                                                    subscriberEducation.EducationHistoryGuid.Value : Guid.NewGuid();
                            updateEducationHistory.IsDeleted = 0;
                            updateEducationHistory.SubscriberId = subscriber.SubscriberId;
                            updateEducationHistory.StartDate = (DateTime?)null;
                            updateEducationHistory.EndDate = (DateTime?)null;
                            updateEducationHistory.DegreeDate = (DateTime?)null;
                            updateEducationHistory.RelevantYear = subscriberEducation.RelevantYear;
                            updateEducationHistory.EducationalDegree = educationalDegree ?? new EducationalDegree
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                EducationalDegreeGuid = Guid.NewGuid(),
                                IsDeleted = 0,
                                Degree = subscriberEducation.EducationalDegree
                            };
                            updateEducationHistory.EducationalInstitution = educationalInstitution ?? new EducationalInstitution
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                EducationalInstitutionGuid = Guid.NewGuid(),
                                IsDeleted = 0,
                                Name = subscriberEducation.Institution
                            };
                            updateEducationHistory.EducationalDegreeType = verifiedEducationalDegreeTypes
                                                                          .FirstOrDefault(edt => edt.EducationalDegreeTypeGuid == subscriberEducation.EducationalDegreeTypeGuid);
                        }

                    }

                }
            }

            //await _dbContext.SaveChangesAsync();

            //list of all incoming valid guids in the request.
            var incomingSubscriberTrainingHistoryGuids = subscriberEducationAssessmentsDto.TrainingHistories
                        .Where(st => st.SubscriberTrainingGuid.HasValue && st.SubscriberTrainingGuid.Value != Guid.Empty)
                        .Select(st => (Guid)st.SubscriberTrainingGuid).ToList();

            //Perform soft delete
            var subscriberTrainingHistoryToDelete = subscriberTrainingHistory
                .Where(st => !incomingSubscriberTrainingHistoryGuids.Contains(st.SubscriberTrainingGuid))
                .ToList();

            foreach (var trainingsToDelete in subscriberTrainingHistoryToDelete)
            {
                subscriberTrainingHistory
                    .FirstOrDefault(seh => seh.SubscriberTrainingGuid == trainingsToDelete.SubscriberTrainingGuid)
                    .IsDeleted = 1;
            }

            if (subscriberEducationAssessmentsDto.TrainingHistories != null && subscriberEducationAssessmentsDto.TrainingHistories.Count > 0)
            {

                foreach (var subscriberTraining in subscriberEducationAssessmentsDto.TrainingHistories)
                {
                    if (!subscriberTraining.SubscriberTrainingGuid.HasValue || subscriberTraining.SubscriberTrainingGuid.Value == Guid.Empty)
                    {

                        _dbContext.SubscriberTraining.Add(new SubscriberTraining
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            SubscriberTrainingGuid = subscriberTraining.SubscriberTrainingGuid.HasValue && subscriberTraining.SubscriberTrainingGuid.Value != Guid.Empty ?
                                                     subscriberTraining.SubscriberTrainingGuid.Value : Guid.NewGuid(),
                            IsDeleted = 0,
                            SubscriberId = subscriber.SubscriberId,
                            RelevantYear = subscriberTraining.RelevantYear,
                            TrainingInstitution = subscriberTraining.TrainingInstitution,
                            TrainingName = subscriberTraining.TrainingName,
                            TrainingTypeId = trainingType.FirstOrDefault(tt => tt.TrainingTypeGuid == subscriberTraining.TrainingTypeGuid).TrainingTypeId
                        });

                    }
                    else
                    {
                        var updateTrainingHistory = subscriberTrainingHistory.FirstOrDefault(seh => seh.SubscriberTrainingGuid == subscriberTraining.SubscriberTrainingGuid.Value);
                        if(updateTrainingHistory != null)
                        {
                            updateTrainingHistory.ModifyDate = DateTime.UtcNow;
                            updateTrainingHistory.SubscriberTrainingGuid = subscriberTraining.SubscriberTrainingGuid.HasValue && subscriberTraining.SubscriberTrainingGuid.Value != Guid.Empty ?
                                                                           subscriberTraining.SubscriberTrainingGuid.Value : Guid.NewGuid();
                            updateTrainingHistory.IsDeleted = 0;
                            updateTrainingHistory.SubscriberId = subscriber.SubscriberId;
                            updateTrainingHistory.RelevantYear = subscriberTraining.RelevantYear;
                            updateTrainingHistory.TrainingInstitution = subscriberTraining.TrainingInstitution;
                            updateTrainingHistory.TrainingName = subscriberTraining.TrainingName;
                            updateTrainingHistory.TrainingType = trainingType.FirstOrDefault(tt => tt.TrainingTypeGuid == subscriberTraining.TrainingTypeGuid);
                        }
                    }

                }
            }

            await _dbContext.SaveChangesAsync();

        }
    }
}
