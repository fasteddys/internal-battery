using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Publication
    {
        public List<string> authors { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string journal { get; set; }

        public string volume { get; set; }

        public string publisher { get; set; }
        public Date publicationDate { get; set;  }
        public string isbn { get; set; }


    }
}
