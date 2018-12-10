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

namespace SeleniumTests
{
    [TestClass]
    public class TopicTests
    {
        private TestContext testContextInstance;
        private IWebDriver driver;
        private string appURL;

        public TopicTests()
        {
        }

        [TestMethod]
        [TestCategory("Chrome")]
        public void TopicValidJson()
        {
            driver.Navigate().GoToUrl(appURL + "/");
            var topicsPayload = driver.FindElement(By.TagName("pre")).Text;
            bool validJson = false;
            try
            {
                JArray o = JArray.Parse(topicsPayload);
                validJson = true;

            }
            catch { }
      
            Assert.IsTrue(validJson, "Verified that topics are served as valid json");
        }



        [TestMethod]
        [TestCategory("Chrome")]
        public void TopicsServe()
        {
            driver.Navigate().GoToUrl(appURL + "/");

            Assert.IsTrue(driver.PageSource.Contains("topicId"), "Verified that topic are served ");
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
            appURL = "https://ccapidev.azurewebsites.net/api/Topic";

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
