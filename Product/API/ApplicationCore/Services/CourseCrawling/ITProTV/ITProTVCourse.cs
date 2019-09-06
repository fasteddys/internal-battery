using System;
using System.Collections.Generic;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling.ITProTV
{
    public class ITProTVCourse
    {
        public ITProTVCourse(string title, string subtitle, string description, string duration, string overview, List<ItProTVCategory> categories, ISovrenAPI sovrenApi)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.Duration = duration;
            this.Overview = overview;
            this.Skills = Utils.ParseSkillsFromSovren(sovrenApi, title, subtitle, description, overview);
            this.Categories = categories;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public string Duration { get; }
        public string Overview { get; }
        public List<string> Skills { get; }
        public List<ItProTVCategory> Categories { get; }
    }
}
