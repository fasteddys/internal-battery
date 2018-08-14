using System;
using Xamarin.Forms;

namespace UserDetailsClient.Core.Views
{
    public partial class BaseContentPage : ContentPage
    {
        public Helpers.TranslateExtension Culture { get; private set; }
        public BaseContentPage()
        {
            Culture = new Helpers.TranslateExtension();            
        }
    }
}
