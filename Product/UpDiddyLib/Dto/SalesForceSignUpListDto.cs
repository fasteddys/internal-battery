﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SalesForceSignUpListDto : BaseDto
    {
        public Guid SalesForceSignUpListGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName {get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
    }
}
