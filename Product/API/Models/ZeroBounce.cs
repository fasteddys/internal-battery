using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ZeroBounce : BaseModel
    {
        public int ZeroBounceId { get; set; }
        public Guid ZeroBounceGuid { get; set; }
        public string HttpStatus { get; set; }
        public int ElapsedTimeInMilliseconds { get; set; }
        // https://docs.microsoft.com/en-us/ef/core/modeling/backing-field
        private string _response;
        [NotMapped]
        public JObject Response
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(_response) ? "{}" : _response);
            }
            set
            {
                _response = value.ToString();
            }
        }
        public virtual PartnerContact PartnerContact { get; set; }
        public int? PartnerContactId { get; set; }
    }
}
