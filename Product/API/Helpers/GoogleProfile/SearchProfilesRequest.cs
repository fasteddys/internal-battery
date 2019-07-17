using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{


    public class SearchProfilesRequest
    {



        public string parent { get; set; }
        public RequestMetadata requestMetadata { get; set; }
 
        public ProfileQuery profileQuery {get; set; }
        public virtual int? offset { get; set; }
        public virtual string pageToken { get; set; }
        public virtual bool? disableSpellCheck { get; set; }
        public virtual bool? caseSensitiveSort { get; set; }
        // This will not work due to a class issue, see HistogramQuery class for details 
        public HistogramQuery[] histogramQueries { get; set; }

        public string resultSetId { get; set; }

        // Allowed order by values:
        //
        //"relevance desc": By descending relevance, as determined by the API algorithms.
        //"update_time desc": Sort by Profile.update_time in descending order(recently updated profiles first).
        //"create_time desc": Sort by Profile.create_time in descending order(recently created profiles first).
        //"first_name": Sort by PersonName.PersonStructuredName.given_name in ascending order.
        //"first_name desc": Sort by PersonName.PersonStructuredName.given_name in descending order.
        //"last_name": Sort by PersonName.PersonStructuredName.family_name in ascending order.
        //"last_name desc": Sort by PersonName.PersonStructuredName.family_name in ascending order.

        public  string orderBy { get; set; }       
        public  int? pageSize { get; set; }
 
    }




}




