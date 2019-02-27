using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ActionButtonViewModel : ComponentViewModel
    {
        public enum Action { SUBMIT, CTA};
        public Action ButtonAction { get; set; }
        public string Text { get; set; }
        public string SkewDirection { get; set; }
        public string Form { get; set; }
        public string Hyperlink { get; set; }
        public bool FitToText { get; set; }
    }
}
