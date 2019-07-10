using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    // todo: consider moving these constants to their respective domains of usage
    public static class Constants
    {
        public const string TRACKING_KEY_PARTNER_CONTACT = "Contact";
        public const string TRACKING_KEY_ACTION = "Action";
        public const string TRACKING_KEY_CAMPAIGN = "Campaign";
        public const string TRACKING_KEY_CAMPAIGN_PHASE = "CampaignPhase";
        public const string TRACKING_KEY_JOB_APPLICATION = "JobApplication";
        public const string TRACKING_KEY_TINY_ID = "TinyId";

        static public string  SubsriberSessionKey = "Subscriber";
        static public readonly string EMPTY_STRING = "";
        static public readonly string HttpGetClientName = "HttpGetClient";
        static public readonly string HttpPostClientName = "HttpPostClient";
        static public readonly string HttpPutClientName = "HttpPutClient";
        static public readonly string HttpDeleteClientName = "HttpDeleteClient";
        static public readonly string PollyStringCacheName = "PollyStringCacheName";
        static public readonly int PollyStringCacheTimeInMinutes = 5;
        static public readonly string SysLogLogInformationTrue = "true";
        static public readonly string WozVendorName = "WozU";
        static public readonly string RegionCodeUS = "US";
        static public readonly List<String> ValidTextFileExtensions = new List<String>
        {
            "doc", "docx", "odt", "pdf", "rtf", "tex", "txt", "wks", "wps", "wpd"
        };
        static public readonly string NotSpecifedOption = "Not Specified";

        public static class DataFormat
        {
            public static readonly string Json = "Json";
            public static readonly string Xml = "Xml";
        }

        // data sources for subscriber staging store
        public static class DataSource
        {
            public static readonly string Sovren = "Sovren";
            public static readonly string LinkedIn = "LinkedIn";
            public static readonly string CareerCircle = "CareerCircle";
        }

        public static class SignalR
        {
            public static readonly string CookieKey = "ccsignalr_connection_id";
            public static readonly string ResumeUpLoadVerb = "UploadResume";
            public static readonly string ResumeUpLoadAndParseVerb = "ResumeUpLoadAndParseVerb";
        }

        public static class CampaignRebate
        {
            public static readonly string CourseCompletion_Completed_EligibleMsg = "Congratulations on your new skill! Contact customer support to process your rebate";
            public static readonly string CourseCompletion_Completed_NotEligibleMsg = "Congratulations on your new skill! Contact customer support for job oppurtunities";
            public static readonly string CourseCompletion_InProgress_EligibleMsg = "Complete in {0} days for a full rebate";
            public static readonly string CourseCompletion_InProgress_NotEligibleMsg = "Rebate offer expired";
            public static readonly string Employment_Completed_EligibleMsg = "Congratulations on your new skill! Contact customer support for a full rebate if hired by one of our partners within {0} days";
            public static readonly string Employment_InProgress_EligibleMsg = "This course is free when you get hired by one of our partners within {0} days";
            public static class CampaignRebateType
            {
                public static readonly string Employment = "Employment";
                public static readonly string CourseCompletion = "Course completion";
            }

        }

        public static class JobPosting
        {
            public static readonly string ValidationError_CompanyRequiredMsg = "Company required";
            public static readonly string ValidationError_InvalidSecurityClearanceMsg = "Invalid security clearance";
            public static readonly string ValidationError_InvalidIndustryMsg = "Invalid industry";
            public static readonly string ValidationError_InvalidJobCategoryMsg = "Invalid job category";
            public static readonly string ValidationError_InvalidEducationLevelMsg = "Invalid education level";
            public static readonly string ValidationError_InvalidExperienceLevelMsg = "Invalid experience level";
            public static readonly string ValidationError_InvalidEmploymentTypeMsg = "Invalid employment type";
            public static readonly string ValidationError_SubscriberRequiredMsg = "Subscriber is required";
            public static readonly string ValidationError_InvalidDescriptionLength= "Posting Description must contain at least {0} characters";
            public static readonly string ValidationError_JobNotIndexed = "Job has not been indexed";      
        }

        public static class Appsettings
        {
            public static readonly string SendGrid_Transactional_ApiKey = "SysEmail:Transactional:ApiKey";
            public static readonly string SendGrid_Leads_ApiKey = "SysEmail:Leads:ApiKey";
            public static readonly string SendGrid_Marketing_ApiKey = "SysEmail:Marketing:ApiKey";
            public static readonly string SendGrid_InternalLeads_ApiKey = "SysEmail:InternalLeads:ApiKey";
        }

        public enum SendGridAccount
        {
            Transactional,
            Marketing,
            Leads,
            InternalLeads
        }

        public static class CMS
        {
            public static readonly string NULL_RESPONSE = "NULL_RESPONSE";
            public static readonly string RESPONSE_RECEIVED = "RESPONSE_RECEIVED";
        }

        public static class Seo
        {
            public static readonly string META_TITLE = "meta_title";
            public static readonly string META_DESCRIPTION = "meta_description";
            public static readonly string META_KEYWORDS = "meta_keywords";
        }

        public static class EventType
        {
            public static readonly string JobPosting = "Job posting";
        }

        public static class Action
        {
            public static readonly string ApplyJob = "Apply job";
        }
    }
}
