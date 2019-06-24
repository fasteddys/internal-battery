using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class ResumeParseFactory
    {


        public static async Task<bool> ResolveProfileMerge(IRepositoryWrapper repositoryWrapper, UpDiddyDbContext db,  IMapper mapper, ILogger syslog, ResumeParse resumeParse, Subscriber subscriber, string mergeInfo)
        {

            string[] updates = mergeInfo.Split(',');
            // apply each update 
            foreach (string update in updates)
            {
                // wrap in try for fault tolerance
                try
                {
                    if (update.Trim() == string.Empty)
                        continue;

                    string[] updateInfo = update.Split(';');
                    string updateType = updateInfo[0];
                    string updateData = updateInfo[1];
                    string resultGuidStr = string.Empty;
                    ResumeParseResult resumeParseResult = null;
                    if (updateType.StartsWith("rb_"))
                    {
                        resultGuidStr = updateType.Replace("rb_", string.Empty);
                        resumeParseResult = await repositoryWrapper.ResumeParseResultRepository.GetResumeParseResultByGuidAsync(Guid.Parse(resultGuidStr));
                        await _resolveRadioQuestion(repositoryWrapper,  db, subscriber, resumeParseResult, updateData);
                    }
                    else if (updateType.StartsWith("chk_"))
                    {
                        resultGuidStr = updateType.Replace("chk_", string.Empty);
                        resumeParseResult = await repositoryWrapper.ResumeParseResultRepository.GetResumeParseResultByGuidAsync(Guid.Parse(resultGuidStr));
                        await _resolveCheckQuestion(repositoryWrapper, db, subscriber, resumeParseResult, updateData);

                    }
                }
                catch
                {
                    syslog.Log(LogLevel.Information, $"***** ResumeParseFactory.ResolveProfileMerge error resolving parse merge for subscriber : {subscriber.SubscriberGuid} for resume parse  {resumeParse.ResumeParseGuid}");                
                }    
            }
            // marked the subscriber as modified 
            subscriber.ModifyDate = DateTime.UtcNow;
            // mark all unmerged results as ResumeParseStatus.Declined since these items were check boxes questions that were left unchecked and not passed
            // in the mergeInfo data since they did not httpPost on the front end.
            await ResumeParseFactory.DeclineUnMergedParseResults(repositoryWrapper, resumeParse);
            // mark the resume parse record as merged
            resumeParse.ParseStatus = (int)ResumeParseStatus.MergeComplete;
            resumeParse.RequiresMerge = 0;
            resumeParse.ModifyDate = DateTime.UtcNow;
            await repositoryWrapper.ResumeParseRepository.SaveAsync();
            return true;
        }

        /// <summary>
        /// marke all resume results that require merging for the given resumeParse as declined 
        /// </summary>
        /// <param name="repositoryWrapper"></param>
        /// <param name="resumeParse"></param>
        /// <returns></returns>
        public static async Task<bool> DeclineUnMergedParseResults(IRepositoryWrapper repositoryWrapper, ResumeParse resumeParse)
        {
            IList<ResumeParseResult> unMergedResults = await repositoryWrapper.ResumeParseResultRepository.GetResultsRequiringMergeById(resumeParse.ResumeParseId);
            foreach ( ResumeParseResult r in unMergedResults)
                r.ParseStatus = (int) ResumeParseStatus.Declined;

            await repositoryWrapper.ResumeParseResultRepository.SaveResumeParseResultAsync();     
            return true;
        }


            public static async Task<ResumeParseQuestionnaireDto> GetResumeParseQuestionnaire( IRepositoryWrapper repositoryWrapper, IMapper mapper,  ResumeParse resumeParse)
        {
            IList<ResumeParseResult> resumeParseResults = await repositoryWrapper.ResumeParseResultRepository.GetResultsRequiringMergeById(resumeParse.ResumeParseId);

            ResumeParseQuestionnaireDto resumeParseQuestionaireDto = new ResumeParseQuestionnaireDto()
            {
                ResumeParseGuid = resumeParse.ResumeParseGuid,
                ContactQuestions = new List<ResumeParseResultDto>(),
                EducationHistoryQuestions = new List<ResumeParseResultDto>(),
                WorkHistoryQuestions = new List<ResumeParseResultDto>(),
                Skills = new List<ResumeParseResultDto>()
            };

            foreach ( ResumeParseResult rpr in resumeParseResults)
            {
                if (rpr.ProfileSectionId == (int)ResumeParseSection.ContactInfo)
                    resumeParseQuestionaireDto.ContactQuestions.Add(mapper.Map<ResumeParseResultDto>(rpr) );
                else if (rpr.ProfileSectionId == (int)ResumeParseSection.EducationHistory)
                    resumeParseQuestionaireDto.EducationHistoryQuestions.Add(mapper.Map<ResumeParseResultDto>(rpr));
                else if (rpr.ProfileSectionId == (int)ResumeParseSection.WorkHistory)
                    resumeParseQuestionaireDto.WorkHistoryQuestions.Add(mapper.Map<ResumeParseResultDto>(rpr));
                else if (rpr.ProfileSectionId == (int)ResumeParseSection.Skills)
                    resumeParseQuestionaireDto.Skills.Add(mapper.Map<ResumeParseResultDto>(rpr));
            }

            // sort skills 
            resumeParseQuestionaireDto.Skills = resumeParseQuestionaireDto.Skills.OrderBy(s => s.ExistingValue).ToList();

            return resumeParseQuestionaireDto;
        }

        #region private helper functions

        // todo - fix this - its ugggly!
        private static async Task<bool> _resolveRadioQuestionReferenceProperty(IRepositoryWrapper repositoryWrapper, UpDiddyDbContext db, Subscriber subscriber, ResumeParseResult resumeParseResult, string info)
        {
            // short circult on existing 
            if ( info == "existing")
            {
                resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                 await db.SaveChangesAsync();
                return true;
            }

            if (resumeParseResult.TargetTypeName == "Subscriber.StateCode")
            {
                switch (info)
                {
                    case "parsed":
                        State state = StateFactory.GetStateByStateCode(db, resumeParseResult.ParsedValue.Trim());
                        if (state != null)
                        {
                            subscriber.StateId = state.StateId;
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Merged;

                        }
                        else
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;

                        break;
                    case "neither":             
                        subscriber.StateId = null; 
                        resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                        break;
                }

            }
            else if (resumeParseResult.TargetTypeName == "SubscriberEducationHistory.EducationalDegreeId")
            {
                SubscriberEducationHistory subscriberEducationHistory = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(db, resumeParseResult.ExistingObjectGuid);
                EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(db, resumeParseResult.ParsedValue.Trim());

                switch (info)
                {
                    case "parsed":
                        
                        if (educationalDegree != null && subscriberEducationHistory != null)
                        {
                            subscriberEducationHistory.EducationalDegreeId = educationalDegree.EducationalDegreeId;
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Merged;
                        }
                        else
                        {
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                            string msg = string.Empty;
                            if (subscriberEducationHistory == null)
                                msg = $"Could not locate SubscriberEducationHistory with guid of {resumeParseResult.ExistingObjectGuid}";

                            resumeParseResult.ProcessingMessage = msg;
                        }

                        break;
                    case "neither":  
                        educationalDegree =  await EducationalDegreeFactory.GetOrAdd(db, Constants.NotSpecifedOption );
                        subscriberEducationHistory.EducationalDegreeId = educationalDegree.EducationalDegreeId;
                        resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                        break;
                }

            }
            else if (resumeParseResult.TargetTypeName == "SubscriberEducationHistory.EducationalDegreeTypeId")
            {
                SubscriberEducationHistory subscriberEducationHistory = subscriberEducationHistory = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(db, resumeParseResult.ExistingObjectGuid);
                if (subscriberEducationHistory == null)
                {
                    resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                    resumeParseResult.ProcessingMessage = $"Could not locate SubscriberEducationHistory with guid of {resumeParseResult.ExistingObjectGuid}";
                    await db.SaveChangesAsync();
                    return false;
                }             
            
                EducationalDegreeType educationalDegreeType = null;               

                switch (info)
                {
                    case "parsed":
                       
                       educationalDegreeType = EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(db, resumeParseResult.ParsedValue.Trim());
                        if (educationalDegreeType != null )
                        {
                            subscriberEducationHistory.EducationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Merged;

                        }
                        else
                        {
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                            string msg = string.Empty;
                     
                            if (educationalDegreeType == null)
                                msg += $" Could not locate EducationalDegreeType {resumeParseResult.ParsedValue.Trim()}";

                            resumeParseResult.ProcessingMessage = msg;
                        }

                        break;
                    case "neither":
                        educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(db, Constants.NotSpecifedOption);
                        subscriberEducationHistory.EducationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;
                        resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                        break;
                }


            }

            int numUpdates = await db.SaveChangesAsync();


            return true;
        }


         private static async Task<bool> _resolveRadioQuestion(IRepositoryWrapper repositoryWrapper, UpDiddyDbContext db, Subscriber subscriber, ResumeParseResult resumeParseResult, string info)
        {
            try
            {
                // default to updating the subscriber object 
                Object obj = subscriber;
                Type type = Type.GetType("UpDiddyApi.Models.Subscriber");

                // todo find better way for these referenced objects 
                if (resumeParseResult.TargetTypeName.Contains('.' ) )
                {
                    return await _resolveRadioQuestionReferenceProperty(repositoryWrapper, db, subscriber, resumeParseResult, info);
                }

                if (resumeParseResult.TargetTypeName == "SubscriberWorkHistory")
                {
                    type = Type.GetType("UpDiddyApi.Models.SubscriberWorkHistory");
                    obj = SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(db, resumeParseResult.ExistingObjectGuid);
                }                    
                else if (resumeParseResult.TargetTypeName == "SubscriberEducationHistory")
                {
                    type = Type.GetType("UpDiddyApi.Models.SubscriberEducationHistory");
                    obj = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(db, resumeParseResult.ExistingObjectGuid);
                }
                
                PropertyInfo propertyInfo = type.GetProperty(resumeParseResult.TargetProperty);
 
                if ( obj == null )
                {
                    resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                    resumeParseResult.ProcessingMessage = $"Unable to locate object of type {resumeParseResult.TargetTypeName} with guid {resumeParseResult.ExistingObjectGuid}";
                }
                else
                {
                    switch (info)
                    {
                        case "existing":
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                            break;
                        case "parsed":
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Merged;
                            propertyInfo.SetValue(obj, Utils.ToType(propertyInfo.PropertyType, resumeParseResult.ParsedValue), null);
                            break;
                        case "neither":
                            propertyInfo.SetValue(obj, Utils.ToTypeNullValue(propertyInfo.PropertyType), null);
                            resumeParseResult.ParseStatus = (int)ResumeParseStatus.Declined;
                            break;
                    }

                }
                int numUpdates = await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {

                resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                resumeParseResult.ProcessingMessage = ex.Message;
                await db.SaveChangesAsync();

                return false;
            }


        }



        private static async Task<bool> _resolveCheckQuestion(IRepositoryWrapper repositoryWrapper, UpDiddyDbContext db,  Subscriber subscriber, ResumeParseResult resumeParseResult, string info)
        {
            try
            {
               
                if ( resumeParseResult.TargetTypeName == "SubscriberSkill" )
                {
                    Skill skill = SkillFactory.GetOrAdd(db, info);
                    SubscriberSkillFactory.AddSkillForSubscriber(db, subscriber, skill);
                }
                resumeParseResult.ParseStatus = (int) ResumeParseStatus.Merged;
                int numUpdates = await db.SaveChangesAsync();

                return true;
            }
            catch ( Exception ex )
            {
                 
                 resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                 resumeParseResult.ProcessingMessage = ex.Message;
                 await db.SaveChangesAsync();
              
                return false;
            }


        }

        #endregion


    }
}
