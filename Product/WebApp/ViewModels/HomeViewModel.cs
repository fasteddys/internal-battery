using System;
using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public string TopicName { get; set; }
        public IList<TopicDto> Topics { get; set; }
        public List<JobPostingCountDto> JobCount {get;set;}

        public string GoogleMapsApi {get;set;}

        public string BaseUrl {get;set;}
        public HomeViewModel(IConfiguration _configuration, IList<TopicDto> TopicsFromDto)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.BaseUrl = _configuration["Environment:BaseUrl"];
            this.GoogleMapsApi = _configuration["GoogleMaps:ApiKey"];
            this.Topics = TopicsFromDto;
        }
    }
}
