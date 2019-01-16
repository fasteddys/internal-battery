using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class SubscribersViewModel : BaseViewModel
    {
        public IEnumerable<SubscriberDto> Subscribers { get; set; }
    }
}
