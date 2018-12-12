using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;
using System.Linq;

namespace SeleniumTests
{
    public class SubscriberSkillTests
    {
        [Fact]
        public void AddSkill()
        {
            #region  Setup
            bool testPassed = false;
            string[] args = null;
            UpDiddyDbContext db = new UpDiddyDbContextFactory().CreateDbContext(args);
            Skill skill = new Skill()
            {
                SkillName = "TestSkill",
                ONetCode = "11-3021.00"
                
            };
            db.Skill.Add(skill);
            db.SaveChanges();
            #endregion

            #region Test 

            var skill1 = db.Skill
                 .Where( s => s.SkillName == "TestSkill")
                     .FirstOrDefault();

            if ( skill1 != null )           
                testPassed = true;

            #endregion

            #region Teardown

            db.Entry(skill1).State = EntityState.Deleted;
            db.SaveChanges();

            #endregion

            Assert.True(testPassed);
        }


        [Fact]
        public void AddSubscriberSkill()
        {
            #region Setup 
         
            bool testPassed = false;
            string[] args = null;
            string testEmail = "TestSubscriber@CareerCircle.com";
            UpDiddyDbContext db = new UpDiddyDbContextFactory().CreateDbContext(args);
            

            // Add Subscriber
            Guid subscriberGuid = Guid.NewGuid();
            Subscriber subscriber = new Subscriber()
            {
                SubscriberGuid = subscriberGuid,
                Email = testEmail
            };
            db.Subscriber.Add(subscriber);

            // Add Skill 
            Skill skill = new Skill()
            {
                SkillName = "TestSkill",
                ONetCode = "11-3021.00"
            };
            db.Skill.Add(skill);
            // Save Changes 
            db.SaveChanges();

            var skill1 = db.Skill
                 .Where(s => s.SkillName == "TestSkill")
                     .FirstOrDefault();

            var subscriber1 = db.Subscriber
            .Where(s => s.Email == testEmail)
                .FirstOrDefault();
            #endregion

            #region Test 
            SubscriberSkill subscriberSkilll = new SubscriberSkill()
            {
                SubscriberId = subscriber1.SubscriberId,
                SkillId = skill1.SkillId
            };

            db.SubscriberSkill.Add(subscriberSkilll);
            db.SaveChanges();

            SubscriberSkill subscriberSkill1 = db.SubscriberSkill
                .Where(s => s.SkillId == skill1.SkillId && s.SubscriberId == subscriber1.SubscriberId)
                .FirstOrDefault();

            if (subscriberSkill1 != null)
                testPassed = true;

            #endregion

            #region Teardown
            
            db.Entry(skill1).State = EntityState.Deleted;
            db.Entry(subscriber1).State = EntityState.Deleted;
            db.Entry(subscriberSkill1).State = EntityState.Deleted;
            db.SaveChanges();

            #endregion

            Assert.True(testPassed);
        }


        [Fact]
        public void AddDuplicateSkill()
        {
            #region  Setup

            bool testPassed = false;
            string[] args = null;
            UpDiddyDbContext db = new UpDiddyDbContextFactory().CreateDbContext(args);
            Skill skill = new Skill()
            {
                SkillName = "TestSkill",
                ONetCode = "11-3021.00"
            };

            db.Skill.Add(skill);
            db.SaveChanges();

            #endregion

            #region Test 

            try
            {
                Skill skill1 = new Skill()
                {
                    SkillName = "TestSkill",
                    ONetCode = "11-3021.00"

                };
                db.Skill.Add(skill1);
                db.SaveChanges();
            }
            catch
            {
                testPassed = true;
            }



            #endregion

            #region Teardown

            // Some strangeness here.  Had to bake a new db context to delete the first skill 
            UpDiddyDbContext db1 = new UpDiddyDbContextFactory().CreateDbContext(args);
            var skill3 = db1.Skill
                .Where(s => s.SkillName == "TestSkill")
                .FirstOrDefault();

            db1.Entry(skill3).State = EntityState.Deleted;
            db1.SaveChanges();

            #endregion

            Assert.True(testPassed);
        }



    }
}
