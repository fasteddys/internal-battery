using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TagRepository : UpDiddyRepositoryBase<Tag>, ITagRepository
    {
        public TagRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
