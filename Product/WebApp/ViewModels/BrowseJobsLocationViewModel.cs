using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class BrowseJobsLocationViewModel : BaseViewModel
    {
        public IList<LocationItem> Locations { get; set; }
        public enum ReactComponent { BrowseJobsByStates, BrowseJobsByCity }
        public ReactComponent Component { get; set; }

    }

    public class LocationItem
    {
        public string Location { get; set; }
        public string Url { get; set; }
    }
}
