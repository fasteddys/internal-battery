using System;
using System.ComponentModel;

namespace UpDiddyApi.ApplicationCore.Services.Identity
{
    public enum Role
    {
        [Description("Recruiter")]
        Recruiter = 0,
        [Description("Career Circle Administrator")]
        Administrator = 1,
        [Description("Hiring Manager")]
        HiringManager = 2
    }
}