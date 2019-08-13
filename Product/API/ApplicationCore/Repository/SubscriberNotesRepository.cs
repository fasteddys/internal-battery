using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
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
    }
}
