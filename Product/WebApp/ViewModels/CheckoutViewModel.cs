using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class CheckoutViewModel : BaseViewModel
    {
        public SubscriberDto Subscriber { get; set; }

        public CheckoutViewModel(SubscriberDto subscriber)
        {
            this.Subscriber = subscriber;
        }
    }
}
