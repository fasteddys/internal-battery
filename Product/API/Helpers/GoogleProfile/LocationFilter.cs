using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class LocationFilter
    {
        public string address { get; set; }
        public string regionCode { get; set; }
        public LatLng latLng { get; set; }

        public double distanceInMiles { get; set; }

        public int telecommutePreference { get; set;}

        public bool negated { get; set; }



    }
}
