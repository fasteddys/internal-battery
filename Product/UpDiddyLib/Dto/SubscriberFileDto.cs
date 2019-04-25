using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberFileDto
    {
        public Guid SubscriberFileGuid { get; set; }
        public string BlobName { get; set; }
        public DateTime CreateDate { get; set; }
        public string SimpleName { get; set; }
    }
}
