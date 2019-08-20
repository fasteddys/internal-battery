﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class SignUpViewModel : ComponentViewModel
    {
        public string SkewDirection { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", ErrorMessage = "Invalid email address. Please update your supplied email and try again.")]
        public string Email { get; set; }
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string Password { get; set; }
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)|(?=.*[a-z])(?=.*[A-Z])(?=.*[^A-Za-z0-9])|(?=.*[a-z])(?=.*\d)(?=.*[^A-Za-z0-9])|(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]))([A-Za-z\d@#$%^&*\-_+=[\]{}|\\:',?/`~""();!]|\.(?!@)){8,16}$", ErrorMessage = "Re-entered password must be 8-16 characters, containing 3 out of 4 of the following: Lowercase characters, uppercase characters, digits (0-9), and one or more of the following symbols: @ # $ % ^ & * - _ + = | ' , ? / ` ~ () ; .")]
        public string ReenterPassword { get; set; }
        public Guid PartnerContactGuid { get; set; }
        public Guid? CampaignGuid { get; set; }
        public string CampaignPhase { get; set; }
        public bool IsExpressSignUp { get; set; }
        public string ObfuscatedEmail { get; set; }
        public Guid PartnerGuid { get; set; }
        public string SignUpButtonText{get;set;}
        public Guid? CourseGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"(^$)|(^\d{10}$)", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        public bool IsWaitList {get;set;}
    }
}


