using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class SearchProfileResponse
    {
        public long estimatedTotalSize { get; set; }
        public SpellingCorrection spellCorrection { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
        public string nextPageToken { get; set; }
        public List<HistogramQueryResult> histogramqueryResults { get; set; }
        
        public List<SummarizedProfile> summarizedProfiles { get; set; }

        public string resultSetId { get; set; } 


 

    }
}
