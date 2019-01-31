using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ActionButtonViewModel : BandViewModel
    {
        public string Text { get; set; }
        public string SkewDirection { get; set; }
    }
}
