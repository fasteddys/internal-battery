using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using System.Linq;
using Action = UpDiddyApi.Models.Action;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class CareerPathCourseRepository : UpDiddyRepositoryBase<CareerPathCourse>, ICareerPathCourseRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CareerPathCourseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        
    }
}
