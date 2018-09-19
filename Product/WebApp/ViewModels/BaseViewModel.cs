using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.ViewModels
{
    public class BaseViewModel
    {
        public string ImageUrl { get; set; }
        public string Synopsis(string description)
        {
            return "Hello";
        }
    }
}
