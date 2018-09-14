using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class TopicViewModel : BaseViewModel
    {
        public string TopicName { get; set;}
        public IList<TopicDto> Topics { get; set; }
        public TopicViewModel(IConfiguration _configuration, IList<TopicDto> TopicsFromDto)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.Topics = TopicsFromDto;
        }
    }
}
