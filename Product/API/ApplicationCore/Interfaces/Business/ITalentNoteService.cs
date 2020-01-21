using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITalentNoteService
    {
        Task<bool> CreateNote(Guid subscriber, Guid talent, SubscriberNotesDto subscriberNoteDto);
        Task<bool> UpdateNote(Guid subscriberGuid, Guid talentGuid, Guid noteGuid, SubscriberNotesDto subscriberNoteDto);
        Task<bool> DeleteNote(Guid subscriberGuid, Guid notGuid);
        Task<SubscriberNotesDto> GetNote(Guid subscriberGuid, Guid notGuid);
        Task<SubscriberNotesListDto> GetNotesForSubscriber(Guid subscriberGuid, Guid talentGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");
    }
}
