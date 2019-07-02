using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EmployerFilter
    {
        public string employer { get; set; }
        public int employerFilterMode { get; set; }
        public bool negated { get; set; }
    }
}
