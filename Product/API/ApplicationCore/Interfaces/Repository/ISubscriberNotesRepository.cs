using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberNotesRepository : IUpDiddyRepositoryBase<SubscriberNotes>
    {
        Task AddNotes(SubscriberNotes subscriberNotes);
        Task UpdateNotes(SubscriberNotes subscriberNotes);
        IQueryable<SubscriberNotes> GetAllSubscriberNotesQueryable();
        Task<SubscriberNotes> GetSubscriberNotesBySubscriberNotesGuid(Guid subscrberNotesGuid);

        Task<List<SubscriberNotes>> GetNotesForSubscriber(Guid subscriberGuid, int limit = 30, int offset = 0, string sort = "CreateDate", string order = "descending");
    }
}
