using UpDiddyLib.Dto;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddy.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public List<JobPostingCountDto> JobCount {get;set;}
        public string BaseUrl {get;set;}
        public HomeViewModel(IConfiguration _configuration)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
            this.BaseUrl = _configuration["Environment:BaseUrl"];
        }
    }
}
