using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class PersonStructuredName
    {
        public string givenName { get; set; }
        public string preferredName { get; set; }
        public string middleInitial { get; set; }
        public string familyName { get; set; }
        public List<string> suffixes { get; set; }
        public List<string> prefixes { get; set;  }

    }
}
