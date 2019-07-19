using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Certification
    {
        public string displayName { get; set; }
        public Date acquireDate { get; set; }
        public Date exipreDate { get; set; }
        public string authority { get; set; }

        public string description { get; set; }
    }
}
