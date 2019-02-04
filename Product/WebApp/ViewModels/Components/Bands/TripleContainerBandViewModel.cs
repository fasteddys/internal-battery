using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class TripleContainerBandViewModel : ComponentViewModel
    {
        public string Header { get; set; }
        public string C1TopValue { get; set; }
        public string C1BottomValue { get; set; }
        public string C2TopValue { get; set; }
        public string C2BottomValue { get; set; }
        public string C3TopValue { get; set; }
        public string C3BottomValue { get; set; }
        
    }
}
