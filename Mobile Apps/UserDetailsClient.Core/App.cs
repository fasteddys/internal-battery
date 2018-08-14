using System;
using System.Globalization;
using Microsoft.Identity.Client;
using Plugin.Multilingual;
using Xamarin.Forms;

namespace UserDetailsClient.Core
{
    public class App : Application
    {
        public static PublicClientApplication PCA = null;

        // Azure AD B2C Coordinates
        public static string Tenant = "digitalbt.onmicrosoft.com";
        public static string ClientID = "163272b1-fbab-44f9-bd20-0488e960597a";
        public static string PolicySignUpSignIn = "B2C_1_SignInSignUp";
        public static string PolicyEditProfile = "B2C_1_Edit";
        public static string PolicyResetPassword = "B2C_1_PasswordReset";

        public static string[] Scopes = { "https://digitalbt.onmicrosoft.com/digitalapi/demo.read"};
        public static string ApiEndpoint = "https://digitalbthello.azurewebsites.net/hello";

        public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
        public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
        public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
        public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";

        public static UIParent UiParent = null;



        public App()
        {
                
            // default redirectURI; each platform specific project will have to override it with its own
            PCA = new PublicClientApplication(ClientID, Authority);
            PCA.RedirectUri = $"msal{ClientID}://auth";

            MainPage = new NavigationPage(new MainPage());

            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;

 
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
