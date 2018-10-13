using Newtonsoft.Json.Linq;
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

            string LogTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // string WozTransResponse = "{\"transactionId\":\"fa7ac26d-0c3d-43b4-ab9d-1b48b2d081b7\",\"clientId\":\"allegis.qa.exeterlms.com\",\"createDateUTC\":1537873016476,\"lastOperationDateUTC\":1537873024607,\"expectedCompletionDateUTC\":253402300800000,\"operation\":\"UserCreate\",\"status\":400,\"message\":\"The operation has completed with the following message: User Email: ikoplowitz@populusgroup.com Id: 5b237d37-ef06-495f-88a9-5ba3ecbb5ec3 registration successful!\",\"resource\":\"{\\\"invitationCode\\\":\\\"a056bc5f - 7d76 - 400d - b6a1 - 7eaa1e84a69f\\\",\\\"registrationUrl\\\":\\\"https://allegis.qa.exeterlms.com/Account/Register?invitationCode=a056bc5f-7d76-400d-b6a1-7eaa1e84a69f\\\",\\\"exeterId\\\":3237,\\\"firstName\\\":\\\"Ian\\\",\\\"lastName\\\":\\\"Koplowitz\\\",\\\"emailAddress\\\":\\\"ikoplowitz@populusgroup.com\\\",\\\"integrationIds\\\":null}\",\"apiVersion\":\"1.0\"}";

            //string WozTransResponse = "{\"transactionId\":\"8a00be19 - 0815 - 419d - 9719 - 8ac4e1d296fa\",\"clientId\":\"allegis.qa.exeterlms.com\",\"createDateUTC\":1537875919749,\"lastOperationDateUTC\":1537875923027,\"expectedCompletionDateUTC\":253402300800000,\"operation\":\"SectionCreate\",\"status\":400,\"message\":\"The operation has completed with the following message: Section Created\",\"resource\":\"{\\\"sectionId\\\":42519,\\\"createDateUTC\\\":1537875921065,\\\"startDateUTC\\\":1468800000,\\\"endDateUTC\\\":1468800000,\\\"isOpen\\\":true,\\\"maxStudents\\\":0,\\\"timeZone\\\":\\\"Eastern Standard Time\\\",\\\"courseCode\\\":\\\"FSWG100\\\",\\\"courseName\\\":\\\"Coding From Scratch\\\"}\",\"apiVersion\":\"1.0\"}";


            string WozTransResponse = "{\"transactionId\":\"5504612d-672e-4836-a536-d2affffa1d42\",\"clientId\":\"allegis.qa.exeterlms.com\",\"createDateUTC\":1537878414074,\"lastOperationDateUTC\":1537878415242,\"expectedCompletionDateUTC\":253402300800000,\"operation\":\"CourseEnrollmentCreate\",\"status\":400,\"message\":\"The operation has completed with the following message: Enrollment Created\",\"resource\":\"{\\\"enrollmentId\\\":17509,\\\"sectionId\\\":42519,\\\"enrollmentStatus\\\":100,\\\"enrollmentDateUTC\\\":1541030400000,\\\"removalDateUTC\\\":null,\\\"exeterId\\\":3237,\\\"firstName\\\":\\\"Ian\\\",\\\"lastName\\\":\\\"Koplowitz\\\",\\\"emailAddress\\\":\\\"ikoplowitz@populusgroup.com\\\",\\\"integrationIds\\\":null}\",\"apiVersion\":\"1.0\"}";



            JObject WozJson = JObject.Parse(WozTransResponse);


            string WozResourceStr = (string)WozJson["resource"];
            var WozResourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(WozResourceStr);
            int EnrollmentId = int.Parse( (string) WozResourceObject.enrollmentId);
            int Section = int.Parse((string)WozResourceObject.sectionId);
            int EnrollmentStatus = int.Parse((string) WozResourceObject.enrollmentStatus);
            int ExeterId = int.Parse( (string) WozResourceObject.exeterId);
            long  CourseEnrollmentTimeStamp = long.Parse( (string) WozResourceObject.enrollmentDateUTC);
            string CourseCode = WozResourceObject.CourseCode;            
            DateTime CourseStartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            CourseStartDate = CourseStartDate.AddMilliseconds(CourseEnrollmentTimeStamp);
     


        //WozCourseEnrollmentId = 0,
        //WozEnrollmentId  
        //SectionId  
        //EnrollmentStatus  
        //ExeterId  
        //EnrollmentDateUTC  


        // 1468800000

        // WozCourseSectionId =                
        // CourseCode =
        // Section =
        // Month =
        // Year =





        WozStudentDto wz = new WozStudentDto();
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



            WozCourseEnrollmentDto wce = new WozCourseEnrollmentDto()
            {

                WozCourseEnrollmentId = 0,
                WozEnrollmentId = 34,
                SectionId = 44,
                EnrollmentStatus = 500,
                ExeterId = 3421,
                EnrollmentDateUTC = 34l
               //  EnrollmentId = -1

            };

            var WozCourseEnrollmentDtoJson = Newtonsoft.Json.JsonConvert.SerializeObject(wce);


            string ResponseJson = "{\"transactionId\":\"87622324-7d8a-4180-a823-7e04c43d02f0\",\"payload\":null,\"payloadType\":null,\"message\":\"The request to modify LMS data was successfully queued for processing.  The status of the request may be monitored with the following transaction identifier: '87622324-7d8a-4180-a823-7e04c43d02f0'.\"}";
            var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

            string x = ResponseObject.transactionId;
 

            Console.WriteLine("Hello World!");
        }
    }
}
