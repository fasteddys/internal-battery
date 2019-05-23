using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterFactory
    {
        public static Recruiter GetRecruiterBySubscriberId(UpDiddyDbContext db, int subscriberId)
        {
            return db.Recruiter
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefault();
        }

        public static Recruiter GetRecruiterBySubscriberGuid(UpDiddyDbContext db, Guid subscriberGuid)
        {
            return db.Recruiter
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefault();
        }

        public static Recruiter GetRecruiterById(UpDiddyDbContext db, int recruiterId)
        {
            return db.Recruiter
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.RecruiterId == recruiterId)
                .FirstOrDefault();
        }

        public static Recruiter CreateRecruiter(string email, string firstName, string lastName, string phoneNumber, Subscriber subscriber)
        {
            Recruiter recruiter = new Recruiter();
            recruiter.CreateDate = DateTime.UtcNow;
            recruiter.CreateGuid = Guid.Empty;
            recruiter.Email = email;
            recruiter.FirstName = firstName;
            recruiter.IsDeleted = 0;
            recruiter.LastName = lastName;
            recruiter.PhoneNumber = phoneNumber;
            recruiter.RecruiterGuid = Guid.NewGuid();
            if (subscriber != null && subscriber.SubscriberId > 0)
                recruiter.SubscriberId = subscriber.SubscriberId;
            return recruiter;
        }

        public static Recruiter GetAddOrUpdate(UpDiddyDbContext db, string email, string firstName, string lastName, string phoneNumber, Subscriber subscriber)
        {
            email = email.Trim();
            Recruiter recruiter = db.Recruiter
                .Where(r => r.IsDeleted == 0 && r.Email == email)
                .FirstOrDefault();

            if (recruiter == null)
            {
                recruiter = CreateRecruiter(email, firstName, lastName, phoneNumber, subscriber);
                db.Recruiter.Add(recruiter);
                db.SaveChanges();
            }
            else if ((!string.IsNullOrWhiteSpace(phoneNumber) && recruiter.PhoneNumber != phoneNumber)
                || (!string.IsNullOrWhiteSpace(firstName) && recruiter.FirstName != firstName)
                || (!string.IsNullOrWhiteSpace(lastName) && recruiter.LastName != lastName))
            {
                recruiter.PhoneNumber = phoneNumber;
                recruiter.FirstName = firstName;
                recruiter.LastName = lastName;
                db.SaveChanges();
            }
            return recruiter;
        }
    }
}
