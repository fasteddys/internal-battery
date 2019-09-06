using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class TalentSubscriberViewModel
    {
        public IEnumerable<SelectListItem> SubscriberSources { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public string SelectedSubscriberSource { get; set; }
        public string SelectedSortOption { get; set; }
    }
}
