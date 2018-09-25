using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.MessageQueue
{
    public class QueueMessage
    {
        public string MessageType { get; set; }  
        public int ProcessingStep { get; set; }
        public int NumSteps { get; set; }
        public int ErrorCount { get; set; }
        public string StatusMessage { get; set; }
        public int StatusCode { get; set; }
    }
}
