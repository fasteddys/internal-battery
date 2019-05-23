using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class BrowseJobsLocationViewModel : JobSearchViewModel
    {
        public IList<DisplayItem> Items { get; set; }
        public string BaseUrl { get; set; }
        public int NumberOfPages { get; set; }
        public int? CurrentPage { get; set; }

    }

    public class DisplayItem
    {
        public string Label { get; set; }
        public string Url { get; set; }
    }
}
