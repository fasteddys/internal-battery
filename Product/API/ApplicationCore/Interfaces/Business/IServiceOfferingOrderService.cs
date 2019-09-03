using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IServiceOfferingOrderService
    {
        bool ProcessOrder(ServiceOfferingOrderDto serviceOfferingOrderDto, Guid subscriberGuid,  ref int statusCode, ref string msg);
    }
}
