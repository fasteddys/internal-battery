using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TagService : ITagService
    {
        private IConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private readonly IMapper _mapper;

        public TagService(
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper)
        {
            _configuration = configuration;
            _repositoryWrapper = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<TagListDto> GetTags(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var tags = await _repositoryWrapper.Tag.GetTags(limit, offset, sort, order);
            if (tags == null)
                throw new NotFoundException("Tags not found");
            return _mapper.Map<TagListDto>(tags);
        }

        public async Task<List<TagDto>> GetTagsByKeyword(string keyword)
        {
            var tags = await _repositoryWrapper.Tag.GetByConditionAsync(x => x.Name.Contains(keyword));
            if (tags == null)
                throw new NotFoundException("Tags not found");

            return _mapper.Map<List<TagDto>>(tags);
        }

        public async Task<TagDto> GetTag(Guid tagGuid)
        {
            var tag = await _repositoryWrapper.Tag.GetByGuid(tagGuid);
            if (tag == null)
                throw new NotFoundException("Tag not found");
            return _mapper.Map<TagDto>(tag);
        }

        public async Task<Guid> CreateTag(TagDto tagDto)
        {
            if (tagDto == null)
                throw new NullReferenceException("TagDto cannot be null");
            Tag tag = _mapper.Map<Tag>(tagDto);
            tag.CreateDate = DateTime.UtcNow;
            tag.TagGuid = Guid.NewGuid();
            await _repositoryWrapper.Tag.Create(tag);
            await _repositoryWrapper.SaveAsync();
            return tag.TagGuid.Value;
        }

        public async Task UpdateTag(Guid tagGuid, TagDto tagDto)
        {
            if (tagDto == null || tagGuid == null || tagGuid == Guid.Empty)
                throw new NullReferenceException("TagDto cannot be null");
            Tag tag = await _repositoryWrapper.Tag.GetByGuid(tagGuid);
            if (tag == null)
                throw new NotFoundException("Tag not found");
            tag.Name = tagDto.Name;
            tag.Description = tagDto.Description;
            tag.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteTag(Guid tagGuid)
        {
            if (tagGuid == null || tagGuid == Guid.Empty)
                throw new NullReferenceException("TagDto cannot be null");
            Tag tag = await _repositoryWrapper.Tag.GetByGuid(tagGuid);
            if (tag == null)
                throw new NotFoundException("Tag not found");
            tag.ModifyDate = DateTime.UtcNow;
            tag.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
        
        public async Task<ProfileTagListDto> GetProfileTagsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var tags = await _repositoryWrapper.Tag.GetProfileTagsForRecruiter(profileGuid, subscriberGuid, limit, offset, sort, order);
            return _mapper.Map<ProfileTagListDto>(tags);
        }

        public async Task DeleteTagsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileTagGuids)
        {
            await _repositoryWrapper.Tag.DeleteTagsFromProfileForRecruiter(subscriberGuid, profileTagGuids);
        }

        public async Task<List<Guid>> AddTagsToProfileForRecruiter(Guid subscriberGuid, List<Guid> tagGuids, Guid profileGuid)
        {
            return await _repositoryWrapper.Tag.AddTagsToProfileForRecruiter(subscriberGuid, tagGuids, profileGuid);
        }
    }
}