using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Workflow
{
    public class WozEnrollmentFlow
    {
        public string WozU(string enrollmentGuid)
        {
            Console.WriteLine("***** - Enroll: " + enrollmentGuid);
            return "***** - Enroll: " + enrollmentGuid;

        }
    }



}
