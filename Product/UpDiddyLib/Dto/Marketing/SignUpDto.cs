﻿using System;

namespace UpDiddyLib.Dto.Marketing
{
    public class SignUpDto : BaseDto
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public Guid? campaignGuid { get; set; }
        public Guid? courseGuid { get; set; }
        public string campaignPhase { get; set; }
        public string referer { get; set; }
        public string verifyUrl { get; set; }
        public string referralCode { get; set; }
        public Guid partnerGuid { get; set; }
        public bool IsWaitList { get; set; }
        public string subscriberSource { get; set; }
    }
}
 