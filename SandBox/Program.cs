using System;
using UpDiddyApi.Models;
using UpDiddyLib;
using UpDiddyLib.Dto;

namespace SandBox
{
    class Program
    {
        static void Main(string[] args)
        {

            WozStudent wz = new WozStudent();
            wz.emailAddress = "email@email.com";
            wz.acceptedTermsOfServiceDocumentId = 1;
            wz.firstName = "Jim";
            wz.lastName = "Brazil";

            VendorStudentLogin vs = new VendorStudentLogin();
            vs.CreateDate = DateTime.Now;
            vs.ModifyDate = DateTime.Now;
            vs.SubscriberId = 3;
            vs.ModifyGuid = Guid.NewGuid();
            vs.CreateGuid = Guid.NewGuid();
            vs.VendorId = 34;
            vs.VendorLogin = "Jims Login";


            // 		VendorStudentLoginJson	"{\"VendorStudentLoginId\":0,\"VendorId\":34,\"SubscriberId\":3,\"VendorLogin\":\"Jims Login\",\"IsDeleted\":0,\"CreateDate\":\"2018-09-24T07:07:57.3698962-04:00\",\"ModifyDate\":\"2018-09-24T07:07:57.3880862-04:00\",\"CreateGuid\":\"c885f084-6b72-44d2-a104-71a5d3e5f330\",\"ModifyGuid\":\"41e6659b-144d-4a6d-bee8-8f36a06bcca0\"}"	string

            var VendorStudentLoginJson = Newtonsoft.Json.JsonConvert.SerializeObject(vs);



            WozCourseSection wcs = new WozCourseSection()
            {
                CourseCode = "test",
                Section = 0,
                Year = 2018,
                Month = 9
            };

            var WozCourseSectionJson = Newtonsoft.Json.JsonConvert.SerializeObject(wcs);

            EnrollmentDto ed = new UpDiddyLib.Dto.EnrollmentDto()
            {
                EnrollmentId = 0,
                EnrollmentGuid = Guid.NewGuid(),
                CourseId = 3,
                SubscriberId = 3,
                DateEnrolled = DateTime.Now,
                PricePaid = 59.95M,
                PercentComplete = 0,
                IsRetake = 0,
                CompletionDate = DateTime.MinValue,
                DroppedDate = DateTime.MinValue,
                EnrollmentStatusId = 1,
                TermsOfServiceFlag = 1

            };
            var EnrollmentDTOJson = Newtonsoft.Json.JsonConvert.SerializeObject(ed);



            string ResponseJson = "{\"transactionId\":\"87622324-7d8a-4180-a823-7e04c43d02f0\",\"payload\":null,\"payloadType\":null,\"message\":\"The request to modify LMS data was successfully queued for processing.  The status of the request may be monitored with the following transaction identifier: '87622324-7d8a-4180-a823-7e04c43d02f0'.\"}";
            var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

            string x = ResponseObject.transactionId;
 

            Console.WriteLine("Hello World!");
        }
    }
}
