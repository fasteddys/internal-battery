using System;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xunit;

namespace SeleniumTests
{
 
    public class TopicTests
    {
      
        private IWebDriver driver;
        private string appURL;

        public TopicTests()
        {
        }

        [Fact]
        public void TopicValidJson()
        {
            string appURL = "https://ccapidev.azurewebsites.net/api/Topic";
            driver = new ChromeDriver();
          //   driver = new InternetExplorerDriver();
            driver.Navigate().GoToUrl(appURL + "/");
            var topicsPayload = driver.FindElement(By.TagName("pre")).Text;
            bool validJson = false;
            try
            {
                JArray o = JArray.Parse(topicsPayload);
                validJson = true;

            }
            catch { }

            driver.Quit();

            Assert.True(validJson);
        }



        [Fact]
        public void TopicsServe()
        {
            string appURL = "https://ccapidev.azurewebsites.net/api/Topic";
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl(appURL + "/");
            bool valid = driver.PageSource.Contains("topicId");
            Assert.True(valid);

            driver.Quit();
        }
 
    }
}
