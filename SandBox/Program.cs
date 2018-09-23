using System;

namespace SandBox
{
    class Program
    {
        static void Main(string[] args)
        {


            string ResponseJson = "{\"transactionId\":\"87622324-7d8a-4180-a823-7e04c43d02f0\",\"payload\":null,\"payloadType\":null,\"message\":\"The request to modify LMS data was successfully queued for processing.  The status of the request may be monitored with the following transaction identifier: '87622324-7d8a-4180-a823-7e04c43d02f0'.\"}";
            var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

            string x = ResponseObject.transactionId;
 

            Console.WriteLine("Hello World!");
        }
    }
}
