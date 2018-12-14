using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UpDiddyApi.Models;

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

        public static ProfileDataStatus ImportLinkedIn(UpDiddyDbContext db, SubscriberProfileStagingStore info, ref string msg)
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
                    
                string xml = info.ProfileData;
                XElement theXML = XElement.Parse(xml);

                // Get list of skill found by Sovren
                var skills = theXML.Descendants()
                     .Where(e => e.Name.LocalName == "Skill")
                     .ToList();
              
                // Iterate over their skills 
                foreach (XElement node in skills )
                {                    
                    string skillName = node.Attribute("name").Value.Trim();                    
                    // Get or create the skill Sovren found
                    Skill skill = Skill.GetOrAdd(db, skillName);
                    // Check to see if the subscriber already has that skill 
                    SubscriberSkill subscriberSkill = SubscriberSkill.GetSkillForSubscriber(db, subscriber, skill);
                    // If the subscriber does not have the skill, add it to their profile 
                    if (subscriberSkill == null)
                        SubscriberSkill.AddSkillForSubscriber(db, subscriber, skill);                        
                }
    

                return ProfileDataStatus.Processed;
            }
            catch (Exception ex)
            {
                msg = $"SubscriberSkill:ImportSovren Exception for SubscriberProfileStagingStore Id = {info.SubscriberProfileStagingStoreId} Error = {ex.Message}";
                return ProfileDataStatus.ProcessingError;
            }

        }

        #endregion

    }
}
