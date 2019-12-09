using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.Helpers
{
    public class CourseUrlHelper
    {
        public static void SetVendorAndThumbnailUrl<T>(List<T> courses, IConfiguration config) where T : CourseBaseDto
        {
            foreach (var course in courses)
            {
                SetUrl(course, config);
            }
        }

        public static void SetVendorAndThumbnailUrl(CourseBaseDto course, IConfiguration config)
        {
            SetUrl(course, config);
        }

        public static void SetUrl(CourseBaseDto course, IConfiguration config)
        {
            course.ThumbnailUrl = config["StorageAccount:AssetBaseUrl"] + "Course/" + course.ThumbnailUrl;
            course.VendorLogoUrl = config["StorageAccount:AssetBaseUrl"] + "Vendor/" + course.VendorLogoUrl;
        }
    }
}
