using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class ColumnSkewBandViewModel : BandViewModel
    {
        public string SkewDirection { get; set; }
        public string Header { get; set; }
        public List<ColumnSkewBandElement> Elements { get; set; }

        public class ColumnSkewBandElement
        {
            public string ElementText { get; set; }
            public bool IsImage { get; set; }
            public ColumnSkewBandElement(string _Text, bool _IsImage)
            {
                ElementText = _Text;
                IsImage = _IsImage;
            }
        }
    }
}
