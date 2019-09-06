using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TagCourseRepository : UpDiddyRepositoryBase<TagCourse>, ITagCourseRepository
    {
        public TagCourseRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
