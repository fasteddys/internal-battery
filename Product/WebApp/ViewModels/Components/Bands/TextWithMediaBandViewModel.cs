using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class TextWithMediaBandViewModel : ComponentViewModel
    {
        public string Header { get; set; }
        public string MediaUrl { get; set; }
        public string VideoThumbnailUrl { get; set; }
        public List<String> Descriptions { get; set; }
        public bool SwitchOrder { get; set; }
        public enum MediaType { IMAGE, VIDEO};
        public MediaType Media { get; set; }
        public enum Breakpoint { TABLET, MOBILE}
        public Breakpoint ChangeOn { get; set; }
        public bool IncludeCTA { get; set; }
    }
}
