using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class CareerPathService : ICareerPathService
    {
        private IRepositoryWrapper _repositoryWrapper { get; set; }
        private readonly IMapper _mapper;

        public CareerPathService(
            IRepositoryWrapper repository,
            IMapper mapper)
        {
            _repositoryWrapper = repository;
            _mapper = mapper;
        }

        public async Task<List<CareerPathDto>> GetCareerPaths(int limit, int offset, string sort, string order)
        {
            var careerPaths = await _repositoryWrapper.CareerPathRepository.GetByConditionWithSorting(x => x.IsDeleted == 0, limit, offset, sort, order);
            return _mapper.Map<List<CareerPathDto>>(careerPaths);
        }

        public async Task<List<CourseDetailDto>> GetCareerPathCourses(Guid careerPathGuid)
        {
            if (careerPathGuid == null || careerPathGuid == Guid.Empty)
                throw new NullReferenceException("careerPathGuid cannot be null");
            var courses = await _repositoryWrapper.Course.GetCoursesByCareerPathGuid(careerPathGuid);
            if (courses == null)
                throw new NotFoundException("CareerPath not found");
            return _mapper.Map<List<CourseDetailDto>>(courses);
        }

        public async Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetCareerPathSkills(Guid careerPathGuid)
        {
            if (careerPathGuid == null || careerPathGuid == Guid.Empty)
                throw new NullReferenceException("careerPathGuid cannot be null");
            var skills = await _repositoryWrapper.SkillRepository.GetByCareerPathGuid(careerPathGuid);
            if (skills == null)
                throw new NotFoundException("Skills not found");
            return _mapper.Map<List<UpDiddyLib.Domain.Models.SkillDto>>(skills);
        }
    }
}
