using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TalentNoteService : ITalentNoteService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public TalentNoteService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = configuration;
        }



        public async Task<bool> CreateNote(Guid subscriberGuid, Guid talentGuid, SubscriberNotesDto subscriberNoteDto)
        {

            if (subscriberNoteDto == null)
                throw new NullReferenceException("Talent note cannot be null");

            var talent = await _repositoryWrapper.SubscriberRepository.GetByGuid(talentGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");

            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(subscriberGuid);
            if (recruiter == null)
                throw new NotFoundException("Recruiter not found");

            var subscriberNotes = _mapper.Map<SubscriberNotes>(subscriberNoteDto);
            subscriberNotes.SubscriberNotesGuid = Guid.NewGuid();
            subscriberNotes.SubscriberId = talent.SubscriberId;
            subscriberNotes.RecruiterId = recruiter.RecruiterId;
            subscriberNotes.CreateGuid = subscriberGuid;
            BaseModelFactory.SetDefaultsForAddNew(subscriberNotes);
            await _repositoryWrapper.SubscriberNotesRepository.AddNotes(subscriberNotes);   
            return true;
        }


        public async Task<bool> UpdateNote(Guid subscriberGuid, Guid talentGuid, Guid noteGuid, SubscriberNotesDto subscriberNoteDto)
        {

            if (subscriberNoteDto == null)
                throw new NullReferenceException("Talent note cannot be null");

            var talent = await _repositoryWrapper.SubscriberRepository.GetByGuid(talentGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");

            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(subscriberGuid);
            if (recruiter == null)
                throw new NotFoundException("Recruiter not found");
 
            var subscriberNotes = await _repositoryWrapper.SubscriberNotesRepository.GetByGuid(noteGuid);
            if (subscriberNotes == null)
                throw new NotFoundException("Talent note not found");

            if (recruiter.RecruiterId != subscriberNotes.RecruiterId)
                throw new UnauthorizedAccessException("Not owner of the talent note");


            subscriberNotes.Notes = subscriberNoteDto.Notes;
            subscriberNotes.ViewableByOthersInRecruiterCompany = subscriberNoteDto.ViewableByOthersInRecruiterCompany;
            subscriberNotes.ModifyDate = DateTime.UtcNow;
            subscriberNotes.ModifyGuid = subscriberGuid;
            await _repositoryWrapper.SubscriberNotesRepository.SaveAsync();
            return true;
        }


        public async Task<bool> DeleteNote(Guid subscriberGuid, Guid noteGuid)
        {

            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberGuid(subscriberGuid);
            if (recruiter == null)
                throw new NotFoundException("Recruiter not found");

            var subscriberNotes = await _repositoryWrapper.SubscriberNotesRepository.GetByGuid(noteGuid);
            if (subscriberNotes == null)
                throw new NotFoundException("Talent note not found");

            if (recruiter.RecruiterId != subscriberNotes.RecruiterId)
                throw new UnauthorizedAccessException("Not owner of the talent note");

            subscriberNotes.IsDeleted = 1;            
            subscriberNotes.ModifyDate = DateTime.UtcNow; 
            await _repositoryWrapper.SubscriberNotesRepository.SaveAsync();
            
            return true;

        }




        public async Task<SubscriberNotesDto> GetNote(Guid subscriberGuid, Guid noteGuid)
        {


            var subscriberNotes = await _repositoryWrapper.SubscriberNotesRepository.GetByGuid(noteGuid);
            if (subscriberNotes == null)
                throw new NotFoundException("Talent note not found");

            var recruiter = await _repositoryWrapper.RecruiterRepository.GetRecruiterBySubscriberId(subscriberNotes.RecruiterId);
            if (recruiter == null)
                throw new NotFoundException("Recruiter not found");


            // todo implement company wide view privelage if ViewableByOthersInRecruiterCompany = true 
            if (recruiter.RecruiterId != subscriberNotes.RecruiterId)
                throw new UnauthorizedAccessException("Not owner of the talent note");

            SubscriberNotesDto rVal =  _mapper.Map< SubscriberNotesDto>(subscriberNotes);

            // Manually map some items due to some mismatches between the model and the dto  
            rVal.RecruiterGuid = recruiter.RecruiterGuid;
            rVal.RecruiterName = recruiter.FirstName + " " + recruiter.LastName;
            rVal.SubscriberGuid = subscriberGuid;

            return rVal;

        }




        public async Task<List<SubscriberNotesDto>> GetNotesForSubscriber(Guid subscriberGuid, Guid talentGuid, int limit = 30, int offset = 0, string sort = "CreateDate", string order = "descending")
        {

            var talent = await _repositoryWrapper.SubscriberRepository.GetByGuid(talentGuid);
            if (talent == null)
                throw new NotFoundException("Talent not found");
   
            var notes =  await _repositoryWrapper.StoredProcedureRepository.GetSubscriberNotes(subscriberGuid, talentGuid, limit, offset, sort, order);

            return notes;
        }




    }
 


 
}


 
