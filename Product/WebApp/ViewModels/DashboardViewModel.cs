using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public List<NotificationDto> Notifications { get; set; }
        public string DeviceType { get; set; }
    }
}
