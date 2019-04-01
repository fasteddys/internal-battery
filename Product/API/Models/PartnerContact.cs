using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerContact : BaseModel
    {
        public int PartnerId { get; set; }
        public virtual Partner Partner { get; set; }
        public int ContactId { get; set; }
        public virtual Contact Contact { get; set; }
        public Guid? PartnerContactGuid { get; set; }
        public string SourceSystemIdentifier { get; set; }
        // https://docs.microsoft.com/en-us/ef/core/modeling/backing-field
        private string _metadata;
        [NotMapped]
        public JObject Metadata
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(_metadata) ? "{}" : _metadata);
            }
            set
            {
                _metadata = value.ToString();
            }
        }
        public bool? IsBillable { get; set; }
        public List<PartnerContactLeadStatus> PartnerContactLeadStatuses { get; set; } = new List<PartnerContactLeadStatus>();
    }
}
