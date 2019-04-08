using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class PartnerContactFile : BaseModel
    {

        public int PartnerContactFileId { get; set; }
        public Guid PartnerContactFileGuid { get; set; }
        public int PartnerContactId { get; set; }
        public virtual PartnerContact PartnerContact { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string MimeType { get; set; }
        [Required]
        public string Base64EncodedData { get; set; }
        public bool IsBillable { get; set; }
        public List<PartnerContactFileLeadStatus> PartnerContactFileLeadStatuses { get; set; } = new List<PartnerContactFileLeadStatus>();
    }
}
