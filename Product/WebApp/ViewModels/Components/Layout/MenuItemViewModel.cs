using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels.Components.Layout
{
    public class MenuItemViewModel
    {
        public string url { get; set; }
        public string label { get; set; }
        public bool isroot { get; set; }
        public IList<MenuItemViewModel> child_pages { get; set; }
    }
}
