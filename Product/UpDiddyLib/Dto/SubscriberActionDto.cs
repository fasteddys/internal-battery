using System;

namespace UpDiddyLib.Dto
{
    public class SubscriberActionDto
    {
        public int SubscriberActionId { get; set; }
        public Guid SubscriberActionGuid { get; set; }
        public int SubscriberId { get; set; }
        public int ActionId { get; set; }
        public DateTime OccurredDate { get; set; }
        public int? EntityId { get; set; }
        public int? EntityTypeId { get; set; }
    }
}