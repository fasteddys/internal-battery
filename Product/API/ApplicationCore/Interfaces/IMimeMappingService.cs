using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IMimeMappingService
    {
        Task<string> MapAsync(string fileName);
    }
}
