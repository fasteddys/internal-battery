using AutoMapper;
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


        public static async Task<bool> ResolveProfileMerge(IRepositoryWrapper repositoryWrapper, IMapper mapper, Subscriber subscriber, string mergeInfo)
        {

            string[] updates = mergeInfo.Split(',');

            foreach ( string update in updates)
            {
                // wrap in try for fault tolerance
                try
                {
                    string[] updateInfo = update.Split(';');
                    string updateType = updateInfo[0];
                    string updateData = updateInfo[1];
                    string resultGuidStr = string.Empty;
                    ResumeParseResult resumeParseResult = null;
                    if (updateType.StartsWith("rb_"))
                    {
                        resultGuidStr = updateType.Replace("rb_", string.Empty);
                        resumeParseResult = await repositoryWrapper.ResumeParseResultRepository.GetResumeParseResultByGuidAsync(Guid.Parse(resultGuidStr));
                        await _resolveRadioQuestion(repositoryWrapper, subscriber, resumeParseResult, updateData);
                    }
                    else if (updateType.StartsWith("chk_"))
                    {
                         resultGuidStr = updateType.Replace("chk_", string.Empty);
                         resumeParseResult = await repositoryWrapper.ResumeParseResultRepository.GetResumeParseResultByGuidAsync(Guid.Parse(resultGuidStr));
                        await _resolveCheckQuestion(repositoryWrapper, subscriber, resumeParseResult, updateData);
            
                    }
                } 
                // TODO JAB Add logging here 
                catch { }
            }

            // TODO JAB resolve the master resume parse recored 
            // TODO JAB update all unresolved items to ignored 



            return true;
        }

            public static async Task<ResumeParseQuestionnaireDto> GetResumeParseQuestionnaire( IRepositoryWrapper repositoryWrapper, IMapper mapper,  ResumeParse resumeParse)
        {
            IList<ResumeParseResult> resumeParseResults = await repositoryWrapper.ResumeParseResultRepository.GetResumeParseResultsForResumeParseById(resumeParse.ResumeParseId);

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


        private static async Task<bool> _resolveRadioQuestion(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, ResumeParseResult resumeParseResult, string info)
        {
            try
            {
                // default to updating the subscriber object 
                Object obj = subscriber;
                Type type = Type.GetType("UpDiddyApi.Models.Subscriber");

                if (resumeParseResult.TargetTypeName == "SubscriberWorkHistory")
                {
                    type = Type.GetType("UpDiddyApi.Models.SubscriberWorkHistory");
                    obj = SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(repositoryWrapper.DbContext, resumeParseResult.ExistingObjectGuid);
                }                    
                else if (resumeParseResult.TargetTypeName == "SubscriberEducationHistory")
                {
                    type = Type.GetType("UpDiddyApi.Models.SubscriberEducationHistory");
                    obj = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(repositoryWrapper.DbContext, resumeParseResult.ExistingObjectGuid);
                }
                
                PropertyInfo propertyInfo = type.GetProperty(resumeParseResult.TargetProperty);

                // TODO JAB Deal with statecode and other indirect properties 
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
                            break;
                        case "parsed":
                            propertyInfo.SetValue(obj, Utils.ToType(propertyInfo.PropertyType, resumeParseResult.ParsedValue), null);
                            break;
                        case "neither":
                            propertyInfo.SetValue(obj, Utils.ToTypeNullValue(propertyInfo.PropertyType), null);
                            break;
                    }

                }

                // todo jab uncomment    resumeParseResult.ParseStatus = (int)ResumeParseStatus.Merged;
                //int numUpdates = await repositoryWrapper.DbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {

                resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                resumeParseResult.ProcessingMessage = ex.Message;
                await repositoryWrapper.DbContext.SaveChangesAsync();

                return false;
            }


        }



        private static async Task<bool> _resolveCheckQuestion(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, ResumeParseResult resumeParseResult, string info)
        {
            try
            {
               
                if ( resumeParseResult.TargetTypeName == "SubscriberSkill" )
                {
                    Skill skill = SkillFactory.GetOrAdd(repositoryWrapper.DbContext, info);
                    SubscriberSkillFactory.AddSkillForSubscriber(repositoryWrapper.DbContext, subscriber, skill);
                }
                resumeParseResult.ParseStatus = (int) ResumeParseStatus.Merged;
                int numUpdates = await repositoryWrapper.DbContext.SaveChangesAsync();

                return true;
            }
            catch ( Exception ex )
            {
                 
                 resumeParseResult.ParseStatus = (int)ResumeParseStatus.Error;
                 resumeParseResult.ProcessingMessage = ex.Message;
                 await repositoryWrapper.DbContext.SaveChangesAsync();
              
                return false;
            }


        }

        #endregion


    }
}
