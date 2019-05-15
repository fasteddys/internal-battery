using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Shared.GoogleJobs
{
    // https://cloud.google.com/talent-solution/job-search/docs/client-event#eventtype
    public enum ClientEventType
    {
        Impression
        , View
        , Bookmark
        , View_Direct
        , Notification
        , Hired
        , Sent_Cv
        , Interview_Granted
        , Application_Start
        , Application_Finish
        , Application_Quick_Submit
        , Application_Redirect
        , Application_Company_Submit
    }
}
