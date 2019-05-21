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
        public List<JobQueryFacetItemDto> List { get; set; }
        public IList<string> States { get; set; }

        // Create list of react components so controller can tell view which to use based on
        // the state of the browse process.
        public static IList<string> ReactComponents { get; set; }

    }
}
