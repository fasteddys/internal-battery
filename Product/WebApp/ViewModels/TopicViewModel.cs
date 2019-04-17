using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace UpDiddy.ViewModels
{
    public class TopicViewModel : BaseViewModel
    {
        [JsonProperty("topic_title")]
        public string TopicName { get; set;}
        public TopicDto Topic { get; set; }
        public IList<CourseDto> Courses { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }

        public TopicViewModel(IConfiguration _configuration,IList<CourseDto> courses, TopicDto topic)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Courses = courses;
            this.Topic = topic;
        }
    }
}
