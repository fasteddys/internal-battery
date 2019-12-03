using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberNotesRepository : UpDiddyRepositoryBase<SubscriberNotes>, ISubscriberNotesRepository
    {


        private readonly UpDiddyDbContext _dbContext;
        public SubscriberNotesRepository(UpDiddyDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddNotes(SubscriberNotes subscriberNotes)
        {
            await Create(subscriberNotes);
            await SaveAsync();
        }

        public IQueryable<SubscriberNotes> GetAllSubscriberNotesQueryable()
        {
            return GetAll();
        }

        public async Task<SubscriberNotes> GetSubscriberNotesBySubscriberNotesGuid(Guid subscrberNotesGuid)
        {
            var querableSubscriberNotes = GetAll();
            var subscriberNotesResult = await querableSubscriberNotes
                            .Where(s => s.IsDeleted == 0 && s.SubscriberNotesGuid == subscrberNotesGuid)
                            .ToListAsync();

            return subscriberNotesResult.Count == 0 ? null : subscriberNotesResult[0];
        }

        public async Task UpdateNotes(SubscriberNotes subscriberNotes)
        {
            Update(subscriberNotes);
            await SaveAsync();
        }




        public async Task<List<SubscriberNotes>> GetNotesForSubscriber(Guid subscriberGuid, int limit = 30, int offset = 0, string sort = "CreateDate", string order = "descending")
        {

            PropertyInfo sortProp = typeof(SubscriberNotes).GetProperty(sort, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            List<SubscriberNotes> entity = new List<SubscriberNotes>();
            switch (order)
            {
                case "descending":
                    entity = await _dbContext.SubscriberNotes
              //      .Include(t => t.Recruiter)
             //       .ThenInclude(s => s.Subscriber)
              //      .Include( s=> s.Subscriber)
                    .AsNoTracking()
                    .OrderByDescending(x => sortProp.GetValue(x, null))
                    //.Where(s => s.Subscriber.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                       .Where(s => s.IsDeleted == 0)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
                    break;
                default:
                    entity = await _dbContext.SubscriberNotes
              //       .Include(t => t.Recruiter)
              //      .ThenInclude(s => s.Subscriber)
                //    .Include(s => s.Subscriber)
                    .AsNoTracking()
                    .OrderBy(x => sortProp.GetValue(x, null))
                    //.Where(s => s.Subscriber.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                    .Where(s => s.IsDeleted == 0)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
                    break;
            }

            return entity;

        }
    }
}
