using UpDiddyLib.Dto.User;

namespace UpDiddyLib.Dto
{
    public class ServiceOfferingTransactionDto
    {
        public ServiceOfferingOrderDto ServiceOfferingOrderDto { get; set; }
        public BraintreePaymentDto BraintreePaymentDto { get; set; }
        public CreateUserDto CreateUserDto { get; set; }
    }
}
