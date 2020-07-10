using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICommuteDistancesService
    {

        Task<CommuteDistancesDto> GetCommuteDistances(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending");

        Task<CommuteDistanceDto> GetCommuteDistance(Guid commuteDistanceGuid);
    }
}
