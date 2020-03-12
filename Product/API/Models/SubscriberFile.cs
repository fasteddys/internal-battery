using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    // todo: add name, filetype (resume, other), maybe version
    public class SubscriberFile : BaseModel
    {

        public SubscriberFile()
        {
            SubscriberFileGuid = Guid.NewGuid();
        }
        public int SubscriberFileId { get; set; }
        public Guid SubscriberFileGuid { get; set; }
        public int SubscriberId { get; set; }
        [JsonIgnore]
        public Subscriber Subscriber { get; set; }
        public string BlobName { get; set; }
        public string MimeType { get; set; }

        public string SimpleName
        {
            get
            {
                string pattern = "(^[^_]+_)(.*)";
                if (string.IsNullOrWhiteSpace(BlobName))
                    return null;
                MatchCollection matches = Regex.Matches(BlobName, pattern);
                if (matches.Count == 0)
                    return BlobName;

                return matches[0].Groups[2].Value;
            }
        }
    }
}
