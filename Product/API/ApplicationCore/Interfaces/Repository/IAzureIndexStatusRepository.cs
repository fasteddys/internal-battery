using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IAzureIndexStatusRepository : IUpDiddyRepositoryBase<AzureIndexStatus>
    {
       Task<AzureIndexStatus> GetAzureIndexStatusByName(string name);
    }
}
