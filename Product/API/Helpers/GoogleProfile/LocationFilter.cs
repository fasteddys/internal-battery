using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class LocationFilter
    {
        public string Address { get; set; }
        public string RegionCode { get; set; }
        public LatLng LatLng { get; set; }

        public double DistanceInMiles { get; set; }

        public int TelecommutePreference { get; set;}

        public bool Negated { get; set; }



    }
}
