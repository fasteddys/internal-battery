using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore.Extensions;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class EnrollmentRepository : UpDiddyRepositoryBase<Enrollment>, IEnrollmentRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public EnrollmentRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int> GetEnrollmentsCountByStartEndDates(DateTime? startDate=null, DateTime? endDate=null)
        {
             //get queryable object for subscribers
            var queryableEnrollments=GetAll();

             if(startDate.HasValue)
            {
                queryableEnrollments=queryableEnrollments.Where(e=>e.CreateDate >= startDate);
            }
             if(endDate.HasValue)
            {
                queryableEnrollments=queryableEnrollments.Where(e=>e.CreateDate <endDate);
            }

            return await queryableEnrollments.Where(e=>e.IsDeleted==0).CountAsync();
        }
    }
}