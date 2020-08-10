using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ITraitifyRepository : IUpDiddyRepositoryBase<Traitify>
    {
        Task<Traitify> GetByAssessmentId(string assesmentId);
        Task<Traitify> GetMostRecentAssessmentBySubscriber(Guid subscriber);
    }
}
