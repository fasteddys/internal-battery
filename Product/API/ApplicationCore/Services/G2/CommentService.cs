using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models.G2;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class CommentService : ICommentService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public CommentService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }
        
        public async Task<CommentListDto> GetProfileCommentsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            if (profileGuid == null || profileGuid == Guid.Empty)
                throw new FailedValidationException("profileGuid cannot be null or empty");

            var profileComments = await _repositoryWrapper.CommentRepository.GetProfileCommentsForRecruiter(profileGuid, subscriberGuid, limit, offset, sort, order);

            if (profileComments == null)
                throw new NotFoundException("profile comments not found");
            return _mapper.Map<CommentListDto>(profileComments);
        }

        public async Task<CommentDto> GetCommentForRecruiter(Guid commentGuid, Guid subscriberGuid)
        {
            if (commentGuid == null || commentGuid == Guid.Empty)
                throw new FailedValidationException("commentGuid cannot be null or empty");

            CommentDto commentDto;
            var comment = await _repositoryWrapper.CommentRepository.GetCommentForRecruiter(commentGuid, subscriberGuid);
            if (comment == null)
                throw new NotFoundException("comment not found");

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<Guid> CreateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto)
        {
            if (commentDto == null)
                throw new FailedValidationException("commentDto cannot be null");

            return await _repositoryWrapper.CommentRepository.CreateCommentForRecruiter(subscriberGuid, commentDto);
        }

        public async Task<List<Guid>> CreateCommentsForRecruiter(Guid subscriberGuid, CommentsDto commentsDto)
        {
            if (commentsDto == null)
                throw new FailedValidationException("commentDto cannot be null");

            return await _repositoryWrapper.CommentRepository.CreateCommentsForRecruiter(subscriberGuid, commentsDto);
        }

        public async Task UpdateCommentForRecruiter(Guid subscriberGuid, CommentDto commentDto)
        {
            if (commentDto == null)
                throw new FailedValidationException("commentDto cannot be null");

            await _repositoryWrapper.CommentRepository.UpdateCommentForRecruiter(subscriberGuid, commentDto);
        }

        public async Task DeleteCommentForRecruiter(Guid subscriberGuid, Guid commentGuid)
        {
            if (commentGuid == null || commentGuid == Guid.Empty)
                throw new FailedValidationException("commentGuid cannot be null or empty");

            await _repositoryWrapper.CommentRepository.DeleteCommentForRecruiter(subscriberGuid, commentGuid);
        }
    }
}
