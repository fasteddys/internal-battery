using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class TextWithVideoBandViewModel : BandViewModel
    {
        public string Header { get; set; }
        public string DescriptionLine1 { get; set; }
        public string DescriptionLine2 { get; set; }
        public string VideoUrl { get; set; }
    }
}
