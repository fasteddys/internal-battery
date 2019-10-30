using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterFactory
    {
        public static async Task<Recruiter> GetRecruiterBySubscriberId(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.RecruiterRepository.GetAll()
                .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
                .FirstOrDefaultAsync();
        }

        public static async Task<Recruiter> GetRecruiterBySubscriberGuid(IRepositoryWrapper repositoryWrapper, Guid subscriberGuid)
        {
            return await repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid)
                .FirstOrDefaultAsync();
        }

        public static async Task<Recruiter> GetRecruiterById(IRepositoryWrapper repositoryWrapper, int recruiterId)
        {
            return await repositoryWrapper.RecruiterRepository.GetAll()
                .Include(s => s.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.RecruiterId == recruiterId)
                .FirstOrDefaultAsync();
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

        public static async Task<Recruiter> GetAddOrUpdate(IRepositoryWrapper repositoryWrapper, string email, string firstName, string lastName, string phoneNumber, Subscriber subscriber)
        {
            email = email.Trim();
            Recruiter recruiter = await repositoryWrapper.RecruiterRepository.GetAll()
                .Where(r => r.IsDeleted == 0 && r.Email == email)
                .FirstOrDefaultAsync();

            if (recruiter == null)
            {
                recruiter = CreateRecruiter(email, firstName, lastName, phoneNumber, subscriber);
                await repositoryWrapper.RecruiterRepository.Create(recruiter);
                await repositoryWrapper.SaveAsync();
            }
            else if ((!string.IsNullOrWhiteSpace(phoneNumber) && recruiter.PhoneNumber != phoneNumber)
                || (!string.IsNullOrWhiteSpace(firstName) && recruiter.FirstName != firstName)
                || (!string.IsNullOrWhiteSpace(lastName) && recruiter.LastName != lastName))
            {
                recruiter.PhoneNumber = phoneNumber;
                recruiter.FirstName = firstName;
                recruiter.LastName = lastName;
                await repositoryWrapper.SaveAsync();
            }
            return recruiter;
        }
    }
}
