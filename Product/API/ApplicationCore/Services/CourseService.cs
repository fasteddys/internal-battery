using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public CourseService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task AddCourseAsync(CourseDto courseDto)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteCourseAsync(Guid courseGuid)
        {
            throw new NotImplementedException();
        }

        public async Task EditCourseAsync(CourseDto courseDto)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CourseDto>> GetCoursesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
