using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using AutoMapper;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseLevelService : ICourseLevelService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        public CourseLevelService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<CourseLevelDto> GetCourseLevel(Guid courseLevelGuid)
        {
            if (courseLevelGuid == null || courseLevelGuid == Guid.Empty)
                throw new NullReferenceException("CourseLevelGuid cannot be null");
            IList<CourseLevelDto> rval;
            var courseLevels = await _repositoryWrapper.CourseLevelRepository.GetAllCourseLevels();
            if (courseLevels == null)
                throw new NotFoundException("CourseLevelGuid not found");
            rval = _mapper.Map<List<CourseLevelDto>>(courseLevels);
            return rval?.Where(x => x.CourseLevelGuid == courseLevelGuid).FirstOrDefault();
        }

        public async Task<List<CourseLevelDto>> GetCourseLevels(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courseLevels = await _repositoryWrapper.CourseLevelRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            if (courseLevels == null)
                throw new NotFoundException("CourseLevels not found");
            return _mapper.Map<List<CourseLevelDto>>(courseLevels);
        }

        public async Task CreateCourseLevel(CourseLevelDto courseLevelDto)
        {
            if (courseLevelDto == null)
                throw new NullReferenceException("CourseLevelDto cannot be null");
            var courseLevel = _mapper.Map<CourseLevel>(courseLevelDto);
            courseLevel.CourseLevelGuid = Guid.NewGuid();
            await _repositoryWrapper.CourseLevelRepository.Create(courseLevel);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task UpdateCourseLevel(Guid courseLevelGuid, CourseLevelDto courseLevelDto)
        {
            if (courseLevelDto == null || courseLevelGuid == null || courseLevelGuid == Guid.Empty)
                throw new NullReferenceException("CourseLevelDto and CourseLevelGuid cannot be null");
            var courseLevel = await _repositoryWrapper.CourseLevelRepository.GetByGuid(courseLevelGuid);
            if (courseLevel == null)
                throw new NotFoundException("CourseLevel not found");
            courseLevel.Name = courseLevelDto.Name;
            courseLevel.Description = courseLevelDto.Description;
            courseLevel.SortOrder = courseLevelDto.SortOrder;
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteCourseLevel(Guid courseLevelGuid)
        {
            if (courseLevelGuid == null || courseLevelGuid == Guid.Empty)
                throw new NullReferenceException("courseLevelGuid cannot be null");
            var courseLevel = await _repositoryWrapper.CourseLevelRepository.GetByGuid(courseLevelGuid);
            if (courseLevel == null)
                throw new NotFoundException("CourseLevel not found");
            courseLevel.IsDeleted = 1;
            await _repositoryWrapper.SaveAsync();
        }
    }
}
