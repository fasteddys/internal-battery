using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class PostalAddress
    {
        public int revision { get; set;  } 
        public string regionCode { get; set; }

        public string languageCode { get; set; }

        public string postalCode { get; set; }
        public string sortingCode { get; set; }
        public string administrativeArea { get; set; }
        public string locality { get; set; }
        public string sublocality { get; set; }
        public List<string> addressLines { get; set; }
        public List<string> recipients { get; set; }
        public string organization { get; set; }

    }
}
