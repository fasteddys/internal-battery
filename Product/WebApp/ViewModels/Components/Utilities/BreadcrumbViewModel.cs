using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class BreadcrumbViewModel : ComponentViewModel
    {
        public List<BreadcrumbItem> Breadcrumbs { get; set; }

    }

    public class BreadcrumbItem
    {
        public string PageName { get; set; }
        public string Url { get; set; }
    }
}
