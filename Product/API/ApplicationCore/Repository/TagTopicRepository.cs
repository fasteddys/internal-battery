using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class TagTopicRepository : UpDiddyRepositoryBase<TagTopic>, ITagTopicRepository
    {
        public TagTopicRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
