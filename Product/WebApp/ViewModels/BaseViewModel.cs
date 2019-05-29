

using System;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class BaseViewModel
    {
        public string ImageUrl { get; set; }
        public string Synopsis(string description)
        {
            return "Hello";
        }

        public Guid? LoggedInSubscriberGuid { get; set; }
        public string LoggedInSubscriberName { get; set; }
        public string LoggedInSubscriberEmail { get; set; }
    }
}
