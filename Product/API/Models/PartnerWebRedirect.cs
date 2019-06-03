using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerWebRedirect
    {
        [Key]
        [JsonIgnore]
        public int PartnerId { get; set; }
        public string RelativePath { get; set; }
    }
}
