using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberNotesRepository : UpDiddyRepositoryBase<SubscriberNotes>, ISubscriberNotesRepository
    {
        public SubscriberNotesRepository(UpDiddyDbContext dbContext):base(dbContext)
        {

        }
        public async Task AddNotes(SubscriberNotes subscriberNotes)
        {
            await Create(subscriberNotes);
            await SaveAsync();
        }

        public Task<IQueryable<SubscriberNotes>> GetAllSubscriberNotesQueryable()
        {
            return GetAllAsync();
        }

        public async Task<SubscriberNotes> GetSubscriberNotesBySubscriberNotesGuid(Guid subscrberNotesGuid)
        {
            var querableSubscriberNotes = await GetAllAsync();
            var subscriberNotesResult = querableSubscriberNotes
                            .Where(s => s.IsDeleted == 0 && s.SubscriberNotesGuid == subscrberNotesGuid)
                            .ToList();

            return subscriberNotesResult.Count == 0 ? null : subscriberNotesResult[0];
        }

        public async Task UpdateNotes(SubscriberNotes subscriberNotes)
        {
            Update(subscriberNotes);
            await SaveAsync();
        }
    }
}
