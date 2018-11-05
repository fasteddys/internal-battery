using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EnrollmentFlowDto : BaseDto
    {
        // Add Dto objects here is they're needed in the enrollment flow
        public EnrollmentDto EnrollmentDto { get; set; }
        public BraintreePaymentDto BraintreePaymentDto { get; set; }
    }
}
