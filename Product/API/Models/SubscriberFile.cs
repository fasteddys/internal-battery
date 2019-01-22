using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberFile : BaseModel
    {
        // todo: add name, filetype (resume, other), maybe version
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        [JsonIgnore]
        public Subscriber Subscriber { get; set; }
        public string BlobName { get; set; }

        public string SimpleName
        {
            get
            {
                string pattern = "(^[^_]+_)(.*)";
                MatchCollection matches = Regex.Matches(BlobName, pattern);
                if (matches.Count == 0)
                    return BlobName;

                return matches[0].Groups[2].Value;
            }
        }
    }
}
