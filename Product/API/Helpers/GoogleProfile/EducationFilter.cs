using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EducationFilter
    {
        public string school { get; set; }
        public string fieldOfStudy { get; set; }
        public int degreeType { get; set; }

        public bool negated { get; set; }



    }
}
