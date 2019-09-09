using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    interface IBraintreeService
    {
         bool CapturePayment(BraintreePaymentDto braintreePaymentDto, ref string authInfo, ref int statusCode, ref string msg);
    }
}
