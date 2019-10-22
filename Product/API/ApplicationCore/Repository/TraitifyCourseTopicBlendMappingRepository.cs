using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class TraitifyCourseTopicBlendMappingRepository : UpDiddyRepositoryBase<TraitifyCourseTopicBlendMapping>, ITraitifyCourseTopicBlendMappingRepository
    {
        UpDiddyDbContext _dbContext;

        public TraitifyCourseTopicBlendMappingRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TraitifyCourseTopicBlendMapping> GetByPersonalityTypes(string personalityTypeOne, string personalityTypeTwo)
        {
            return await _dbContext.TraitifyBlendCourseTopicMapping.AsNoTracking().Where(x => x.PersonalityTypeOne == personalityTypeOne && x.PersonalityTypeTwo == personalityTypeTwo).FirstOrDefaultAsync();
        }

    }
}
