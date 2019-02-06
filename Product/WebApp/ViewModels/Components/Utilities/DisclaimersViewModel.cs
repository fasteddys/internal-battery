using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class DisclaimersViewModel : ComponentViewModel
    {
        public List<Disclaimer> Disclaimers { get; set; }
        public class Disclaimer
        {
            public string DisclaimerText { get; set; }
            public Disclaimer(string disclaimerText)
            {
                DisclaimerText = disclaimerText;
            }
        }
    }
}
