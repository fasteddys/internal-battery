using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITraitifyCourseTopicBlendMappingRepository : IUpDiddyRepositoryBase<TraitifyCourseTopicBlendMapping>
    {
        Task<TraitifyCourseTopicBlendMapping> GetByPersonalityTypes(string personalityTypeOne, string personalityTypeTwo);
    }
}
