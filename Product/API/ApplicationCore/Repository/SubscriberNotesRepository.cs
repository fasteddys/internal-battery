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

        public async Task UpdateNotes(SubscriberNotes subscriberNotes)
        {
            Update(subscriberNotes);
            await SaveAsync();
        }
    }
}
