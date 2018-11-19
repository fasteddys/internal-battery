using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
   public class StateViewModel
    {
        public Guid StateGuid { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
