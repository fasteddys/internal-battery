using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class CourseViewModel : BaseViewModel
    {
        public TopicDto Parent { get; set; }
        public CourseDto Course { get; set; }
        public SubscriberDto Subscriber { get; set; }
        public WozTermsOfServiceDto TermsOfService { get; set; }
        public Boolean TermsOfServiceDocId { get; set; }
        public string CourseSlug { get; set; }
        public CourseViewModel(IConfiguration _configuration, CourseDto course, SubscriberDto subscriber, TopicDto parentTopic, WozTermsOfServiceDto tos)
        {
            this.TermsOfService = tos;
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Parent = parentTopic;
            this.Subscriber = subscriber;
            this.Course = course;
        }
    }
}
