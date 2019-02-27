using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class EmailContactDto
    {
        public string email { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string contact_guid { get; set; }
        public string subscriber_guid { get; set; }
    }
}
