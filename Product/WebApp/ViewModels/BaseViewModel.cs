

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
    }
}
