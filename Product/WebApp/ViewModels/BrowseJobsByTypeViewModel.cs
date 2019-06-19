using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class BrowseJobsByTypeViewModel : JobSearchViewModel
    {
        public IList<DisplayItem> Items { get; set; }
        public string BaseUrl { get; set; }
        public int NumberOfPages { get; set; }
        public int? CurrentPage { get; set; }
        public int PaginationRangeLow { get; set; }
        public int PaginationRangeHigh { get; set; }
        public string Header { get; set; }
        public bool HideAllLink { get; set; }
        public BreadcrumbViewModel Breadcrumbs { get; set; }

    }

    public class DisplayItem
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public string Count { get; set; }
    }
}
