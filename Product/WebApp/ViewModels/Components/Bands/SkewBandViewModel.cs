using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class SkewBandViewModel : BandViewModel
    {
        public string SkewDirection { get; set; }
        public string Header { get; set; }
    }
}
