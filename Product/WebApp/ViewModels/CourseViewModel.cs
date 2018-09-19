using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class CourseViewModel : BaseViewModel
    {
        public IList<CourseDto> Courses { get; set; }
        public TopicDto Parent { get; set; }
        public CourseViewModel(IConfiguration _configuration, IList<CourseDto> CoursesFromDto, TopicDto Topic)
        {
            this.Parent = Topic;
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Courses = CoursesFromDto;
        }
    }
}
