using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Dto.Marketing;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingTransactionDto
    {

        public ServiceOfferingOrderDto  ServiceOfferingOrderDto { get; set; }
        public BraintreePaymentDto BraintreePaymentDto { get; set; }
        public SignUpDto SignUpDto { get; set; }

    }
}
