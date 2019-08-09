using System;
using System.Collections.Generic;
using System.Linq;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISalesForceService
    {
        Task AddToWaitList(SalesForceSignUpListDto dto);
    }
}
