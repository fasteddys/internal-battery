using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class VideoViewModel : ComponentViewModel
    {
        public string Url { get; set; }
        public string Thumbnail { get; set; }
    }
}
