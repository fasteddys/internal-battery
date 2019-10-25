using System;
namespace UpDiddyApi.Models
{
    public class TraitifyCourseTopicBlendMapping : BaseModel
    {
        public int TraitifyCourseTopicBlendMappingId { get; set; }
        public Guid TraitifyCourseTopicBlendMappingGuid { get; set; }
        public string PersonalityTypeOne { get; set; }
        public string PersonalityTypeTwo { get; set; }
        public string TopicOneName { get; set; }
        public string TopicOneUrl { get; set; }
        public string TopicOneImgUrl { get; set; }
        public string TopicTwoName { get; set; }
        public string TopicTwoUrl { get; set; }
        public string TopicTwoImgUrl { get; set; }
        public string TopicThreeName { get; set; }
        public string TopicThreeUrl { get; set; }
        public string TopicThreeImgUrl { get; set; }
    }
}
