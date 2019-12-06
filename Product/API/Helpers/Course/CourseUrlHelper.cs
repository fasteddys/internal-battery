using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System;
namespace UpDiddyApi.Helpers
{
    public class CourseUrlHelper
    {
        public static void AssignVendorLogoUrlToCourse<T>(List<T> courses, IConfiguration config) where T : class
        {
            foreach (var course in courses)
            {
                AssignVendorLogoUrlToCourse(course, config);
            }
        }

        public static void AssignVendorLogoUrlToCourse<T>(T course, IConfiguration config) where T : class
        {
            Type t = course.GetType();
            PropertyInfo vendorLogoUrlProp = t.GetProperty("VendorLogoUrl");
            if (vendorLogoUrlProp != null)
            {
                string logoUrl = (string)vendorLogoUrlProp.GetValue(course, null);
                if (!string.IsNullOrEmpty(logoUrl))
                {
                    vendorLogoUrlProp.SetValue(course, SetVendorLogoUrl(logoUrl, config));
                }
            }
        }

        public static string SetVendorLogoUrl(string logoUrl, IConfiguration config)
        {
            return config["StorageAccount:AssetBaseUrl"] + "Vendor/" + logoUrl;
        }
    }
}
