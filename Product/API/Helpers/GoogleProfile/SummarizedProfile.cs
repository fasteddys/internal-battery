using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class SummarizedProfile
    {
        public List<GoogleCloudProfile> profiles { get; set; }
        public GoogleCloudProfile summary { get; set; }
    }
}
