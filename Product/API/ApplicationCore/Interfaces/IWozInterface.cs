using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IWozInterface
    {
        List<DateTime> CheckCourseSchedule(string courseCode);
        MessageTransactionResponse CreateWozStudentLogin(VendorStudentLoginDto StudentLoginDto, string EnrollmentGuid);
        MessageTransactionResponse EnrollStudent(string EnrollmentGuid, ref bool IsInstructorLed);
        Task<WozCourseProgressDto> GetCourseProgress(int SectionId, int WozEnrollmentId);
        MessageTransactionResponse GetSectionForEnrollment(string EnrollmentGuid);
        Task<WozStudentInfoDto> GetStudentInfo(int exeterId);
        WozTermsOfServiceDto GetTermsOfService();
        WozCourseEnrollmentDto ParseWozCourseEnrollmentResource(string EnrollmentGuid, string WozTransactionResponse);
        void ParseWozEnrollmentResource(string WozTransactionResponse, ref string ExeterId, ref string RegistrationUrl);
        WozCourseSectionDto ParseWozSectionResource(string WozTransactionResponse);
        bool ReconcileFutureEnrollment(string EnrollmentGuid);
        MessageTransactionResponse RegisterStudent(string EnrollmentGuid);
        MessageTransactionResponse RegisterStudentInstructorLed(string EnrollmentGuid);
        MessageTransactionResponse SaveCourseSection(WozCourseSectionDto CourseSectionDto, string EnrollmentGuid);
        MessageTransactionResponse SaveWozCourseEnrollment(string EnrollmentGuid, WozCourseEnrollmentDto WozCourseEnrollmentDto);
        MessageTransactionResponse TransactionStatus(string EnrollmentGuid, string TransactionId);
    }
}