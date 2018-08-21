using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace UserDetailsClient.Core.Views
{
    public partial class ChangeCulture : ContentPage
    {
        public ChangeCulture()
        {            
            InitializeComponent();
            picker.Items.Add("English");
            picker.Items.Add("Spanish");
            picker.Items.Add("Portuguese");
            picker.Items.Add("French");
            picker.SelectedItem = CrossMultilingual.Current.CurrentCultureInfo.EnglishName;
        }

        void OnUpdateLangugeClicked(object sender, System.EventArgs e)
        {                             
            CrossMultilingual.Current.CurrentCultureInfo = CrossMultilingual.Current.NeutralCultureInfoList.ToList().First(element => element.EnglishName.Contains(picker.SelectedItem.ToString()));
            AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            App.Current.MainPage = new NavigationPage(new MainPage());
            // Cache culture info
            var CultureJson = JsonConvert.SerializeObject(AppResources.Culture);
            if (App.Current.Properties.ContainsKey("AppResources.Culture"))
                App.Current.Properties["AppResources.Culture"] = CultureJson;
            else 
                App.Current.Properties.Add("AppResources.Culture",CultureJson);
            // Save properties 
            App.Current.SavePropertiesAsync();




        }
 


    }
}
