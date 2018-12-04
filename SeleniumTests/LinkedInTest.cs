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
    public class LinkedInTest
    {
        private TestContext testContextInstance;
        private IWebDriver driver;
        private string appURL;

    

        [TestMethod]
        [TestCategory("Chrome")]
        public void GetLinkedInProfile()
        {

          //  UpDiddyDbContext db = new UpDiddyDbContext();
          //  SubscriberProfileStagingStore pss = new SubscriberProfileStagingStore();
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
     
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            driver.Quit();
        }
    }
}
