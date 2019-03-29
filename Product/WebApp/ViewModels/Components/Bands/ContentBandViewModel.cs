using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ContentBandViewModel : ComponentViewModel
    {
        public const string VIEW_FILE = "Components/Bands/_ContentBand";
        public string Class { get; set; }
        public string Header { get; set; }
        public List<string> Elements { get; set; }
    }
}
