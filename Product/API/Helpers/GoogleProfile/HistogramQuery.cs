using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class HistogramQuery
    {
        // JAB 06-27-2019 Google documentation has the property as the same name as the class which is illegal in
        // c#.  Naming property histogramQuery to get it to compile, not sure of the repercussions
        public string histogramQuery { get; set; }
       
    }
}
