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


        #endregion



    }
}
