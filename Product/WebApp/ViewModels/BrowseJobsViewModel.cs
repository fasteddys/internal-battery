using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class BrowseJobsViewModel : BaseViewModel
    {
        public IList<BrowseJobsByTypeViewModel> ViewModels { get; set; }
    }
}
