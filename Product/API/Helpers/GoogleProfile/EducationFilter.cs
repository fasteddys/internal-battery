using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EducationFilter
    {
        public string School { get; set; }
        public string FieldOfStudy { get; set; }
        public int DegreeType { get; set; }

        public bool Negated { get; set; }



    }
}
