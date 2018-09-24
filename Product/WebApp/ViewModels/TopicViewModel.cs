using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class TopicViewModel : BaseViewModel
    {
        public string TopicName { get; set;}
        public TopicDto Topic { get; set; }
        public IList<CourseDto> Courses { get; set; }
        public TopicViewModel(IConfiguration _configuration,IList<CourseDto> courses, TopicDto topic)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Courses = courses;
            this.Topic = topic;
        }
    }
}
