using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels
{
    public class TopicsViewModel : BaseViewModel
    {
        public IList<TopicViewModel> Topics { get; set; }
        public TopicsViewModel(IConfiguration _configuration)
        {
            this.ImageUrl = _configuration["BaseImageUrl"];
        }
        
    }
}
