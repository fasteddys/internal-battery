using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Workflow
{
    public class WorkFlowMonitor
    {
        public Boolean DoWork()
        {
            Console.WriteLine("***** WorkFlowMonitor Doing Work: "  + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() );
            return true;
        }
        
    }
}
