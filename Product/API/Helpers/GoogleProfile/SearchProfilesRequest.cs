using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{


    public class SearchProfilesRequest
    {



        public string Parent { get; set; }
        public RequestMetadata RequestMetadata { get; set; }
 
        public ProfileQuery ProfileQuery {get; set; }
        public virtual int? Offset { get; set; }
        public virtual string PageToken { get; set; }
        public virtual bool? DisableSpellCheck { get; set; }
        public virtual bool? CaseSensitiveSort { get; set; }
        // This will not work due to a class issue, see HistogramQuery class for details 
        public HistogramQuery[] HistogramQueries { get; set; }

        public string ResultSetId { get; set; }

        // Allowed order by values:
        //
        //"relevance desc": By descending relevance, as determined by the API algorithms.
        //"update_time desc": Sort by Profile.update_time in descending order(recently updated profiles first).
        //"create_time desc": Sort by Profile.create_time in descending order(recently created profiles first).
        //"first_name": Sort by PersonName.PersonStructuredName.given_name in ascending order.
        //"first_name desc": Sort by PersonName.PersonStructuredName.given_name in descending order.
        //"last_name": Sort by PersonName.PersonStructuredName.family_name in ascending order.
        //"last_name desc": Sort by PersonName.PersonStructuredName.family_name in ascending order.

        public  string OrderBy { get; set; }       
        public  int? PageSize { get; set; }
 
    }




}




