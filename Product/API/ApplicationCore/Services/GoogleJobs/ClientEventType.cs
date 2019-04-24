using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Services.GoogleJobs
{
    // https://cloud.google.com/talent-solution/job-search/docs/client-event#eventtype
    public sealed class ClientEventType
    {
        public static string Impression = "IMPRESSION";
        public static string View = "VIEW";
        public static string Bookmark = "BOOKMARK";
        public static string View_Direct = "VIEW_DIRECT";

        public static string Notification = "NOTIFICATION";
        public static string Hired = "HIRED";
        public static string Sent_Cv = "SENT_CV";
        public static string Interview_Granted = "INTERVIEW_GRANTED";

        public static string Application_Start = "APPLICATION_START";
        public static string Application_Finish = "APPLICATION_FINISH";
        public static string Application_Quick_Submit = "APPLICATION_QUICK_SUBMISSION";
        public static string Application_Redirect = "APPLICATION_REDIRECT";
        public static string Application_Company_Submit = "APPLICATION_COMPANY_SUBMIT";
    }
}
