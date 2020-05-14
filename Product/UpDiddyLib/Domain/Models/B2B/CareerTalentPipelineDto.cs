using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class CareerTalentPipelineDto
    {
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ContactPreferences Preferences { get; set; }

        public string Email { get; set; }

        public Dictionary<string, string> Questions = new Dictionary<string, string>();
    }

    public enum ContactPreferences
    {
        Call,
        Text
    }
}
