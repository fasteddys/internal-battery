using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IServiceOfferingOrderService
    {
        bool ProcessOrder(ServiceOfferingTransactionDto serviceOfferingTransactionDto, Guid subscriberGuid,  ref int statusCode, ref string msg);
    }
}
