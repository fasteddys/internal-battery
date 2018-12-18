using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UpDiddyApi.Business.Resume;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Models
{
    public partial class SubscriberSkill : BaseModel
    {

        #region Factory Methods 

        public static bool AddSkillForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Skill skill)
        {
            bool rVal = true;
            try
            {
                SubscriberSkill subscriberSkill = new SubscriberSkill()
                {
                    SkillId = skill.SkillId,
                    SubscriberId = subscriber.SubscriberId,
                    CreateDate = DateTime.Now,
                    CreateGuid = Guid.NewGuid(),
                    ModifyDate = DateTime.Now,
                    ModifyGuid = Guid.NewGuid(),
                    IsDeleted = 0
                };
                db.SubscriberSkill.Add(subscriberSkill);
                db.SaveChanges();
            }
            catch
            {
                rVal = false;
            }
            return rVal;            
        }

        public static SubscriberSkill GetSkillForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Skill skill)
        {
            return db.SubscriberSkill
                .Where(ss => ss.IsDeleted == 0 && ss.SkillId == skill.SkillId && ss.SubscriberId == subscriber.SubscriberId)
                .FirstOrDefault();
        }

        public static ProfileDataStatus ImportLinkedIn(UpDiddyDbContext db, ISovrenAPI sovrenApi , SubscriberProfileStagingStore info, ref string msg)
        {
            try
            {
                // Get the subscriber 
                Subscriber subscriber = Subscriber.GetSubscriberById(db, info.SubscriberId);
                if (subscriber == null)
                {
                    msg = $"SubscriberSkill:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }
 
                // Convert the linked in data to a resume of sorts
                string base64Resume = _LinkedInResume(info.ProfileData);

                // todo: submit as background job, maybe depends on onboarding flow
                String parsedDocument = sovrenApi.SubmitResumeAsync(base64Resume).Result;

                List<string> skills = Utils.ParseSkillsFromHrXML(parsedDocument);
                _AddSubscriberSkills(db, subscriber, skills);

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }

        }

        public static ProfileDataStatus ImportSovren(UpDiddyDbContext db, SubscriberProfileStagingStore info, ref string msg)
        {
            try
            {
                // Get the subscriber 
                Subscriber subscriber = Subscriber.GetSubscriberById(db, info.SubscriberId);
                if (subscriber == null)
                {
                    msg = $"SubscriberSkill:ImportSovren -> Subscriber {info.SubscriberId} was not found";
                    return ProfileDataStatus.ProcessingError;
                }

                List<string> skills = Utils.ParseSkillsFromHrXML(info.ProfileData);
                _AddSubscriberSkills(db, subscriber, skills);
                
                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }

        }

        #endregion


        #region Helper Functions

        private static string _LinkedInResume(string linkedInProfileData)
        {
            StringBuilder tempResume = new StringBuilder();

            JObject o = JObject.Parse(linkedInProfileData);
            JToken firstName = o.SelectToken("$.firstName");
            JToken lastName = o.SelectToken("$.lastName");
            JToken summary = o.SelectToken("$.summary");

            tempResume.Append("Resume for " + firstName.ToString() + " " + lastName.ToString() + Environment.NewLine);
            tempResume.Append("Career Summary: " + summary.ToString() + Environment.NewLine);
            tempResume.Append("Work History" + Environment.NewLine);
            IEnumerable<JToken> positions = o.SelectTokens("$.positions.values[*].summary");
            foreach (JToken workSummary in positions)
                tempResume.Append("Position Summary  " + workSummary.ToString() + Environment.NewLine);

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(tempResume.ToString());
            return System.Convert.ToBase64String(plainTextBytes);

        }



        private static void _AddSubscriberSkills(UpDiddyDbContext db, Subscriber subscriber, List<string> skills)
        {
            foreach (string skillName in skills)
            {
                Skill skill = Skill.GetOrAdd(db, skillName);
                // Check to see if the subscriber already has that skill 
                SubscriberSkill subscriberSkill = SubscriberSkill.GetSkillForSubscriber(db, subscriber, skill);
                // If the subscriber does not have the skill, add it to their profile 
                if (subscriberSkill == null)
                    SubscriberSkill.AddSkillForSubscriber(db, subscriber, skill);
            }
        }

        #endregion

    }
}
