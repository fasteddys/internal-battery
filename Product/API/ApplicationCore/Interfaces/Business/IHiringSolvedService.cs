using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IHiringSolvedService
    {
        Task<bool> RequestParse(int subsriberId, string fileName, string resume64Encoded);
        Task<bool> GetParseStatus(string JobId);
    }
}
