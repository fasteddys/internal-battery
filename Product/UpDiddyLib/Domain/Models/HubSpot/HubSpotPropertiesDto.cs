using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.HubSpot
{
    public class HubSpotPropertiesDto
    {
        public HubSpotPropertiesDto()
        {
            properties = new List<HubSpotProperty>();
        }


        public List<HubSpotProperty> properties { get; set; }
    }



    public class HubSpotProperty
    {
        public string property { get; set; }
        public string value { get; set; }
    }
}
