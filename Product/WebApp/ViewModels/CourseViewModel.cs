using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class CourseViewModel : BaseViewModel
    {
        public IList<CourseDto> Courses { get; set; }
        public String ParentTopicName { get; set; }
        public CourseViewModel(IConfiguration _configuration, IList<CourseDto> CoursesFromDto, string TopicName)
        {
            this.ParentTopicName = TopicName;
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Courses = CoursesFromDto;
        }
    }
}
