using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class EmployerFilter
    {
        public string Employer { get; set; }
        public int EmployerFilterMode { get; set; }
        public bool Negated { get; set; }
    }
}
