using System.Collections.Generic;
using System;
namespace UpDiddyLib.Dto
{
    public class NewNotificationDto : BaseDto
    {
        public NotificationDto NotificationDto { get; set; }
        public List<GroupDto> Groups { get; set; }
        public Guid GroupGuid { get; set; }
    }
}