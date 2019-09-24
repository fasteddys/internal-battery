using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Http;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IServiceOfferingService
    {
        Task<IList<ServiceOfferingDto>> GetAllServiceOfferings();

    }
}
