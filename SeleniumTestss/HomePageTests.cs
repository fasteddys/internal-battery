using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;

namespace SeleniumTestss
{
    [TestClass]
    public class HomePageTests
    {
        private TestContext testContextInstance;
        private IWebDriver driver;
        private string appURL;

        public HomePageTests()
        {
        }

        [TestMethod]
        [TestCategory("Chrome")]
        public void HomePageServes()
        {
            driver.Navigate().GoToUrl(appURL + "/");

            Assert.IsTrue(driver.PageSource.Contains("FAQ"), "Verified that Home page is served"); 
        }


        [TestMethod]
        [TestCategory("Chrome")]
        public void Login()
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            // Set up wait
            TimeSpan timeoutValue = TimeSpan.FromSeconds(10);
            WebDriverWait wait = new WebDriverWait(driver, timeoutValue);

            driver.Navigate().GoToUrl(appURL + "/");
            IWebElement LoginButton = driver.FindElement(By.LinkText("LOGIN/SIGN UP"));
            LoginButton.Click();                   
            IWebElement EmailInput = driver.FindElement(By.Id("logonIdentifier"));
            EmailInput.SendKeys("sejubagama@heros3.com");
            IWebElement PasswordInput = driver.FindElement(By.Id("password"));
            PasswordInput.SendKeys("TestPassword!");
            IWebElement SignInButton = driver.FindElement(By.Id("next"));
            SignInButton.Click();
            wait.Until(d => d.PageSource.ToLower().Contains("my careercircle profile"));

            Assert.IsTrue(driver.PageSource.Contains("My CareerCircle Profile"), "Verified User Login");
        }


  

 

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize()]
        public void SetupTest()
        {
            appURL = "https://careercirclewebapp.azurewebsites.net/home/index";

            string browser = "Chrome";
            switch (browser)
            {
                case "Chrome":
                    driver = new ChromeDriver();
                    break;
                case "Firefox":
                    driver = new FirefoxDriver();
                    break;
                case "IE":
                    driver = new InternetExplorerDriver();
                    break;
                default:
                    driver = new ChromeDriver();
                    break;
            }

        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            driver.Quit();
        }
    }
}
