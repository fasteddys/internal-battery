using System;
using Xunit;
using Moq;
using UpDiddyApi.ApplicationCore;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;
using System.IO;
using System.Reflection;
using UpDiddyLib.Helpers;
using System.Collections.Generic;

namespace API.Tests
{
    public class ResumeParserTests
    {
        [Fact]
        public void Parse48Skills()
        {

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "API.Tests.TestFiles.example-hrxml_48_skills.xml";
            string sampleXML = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                sampleXML = reader.ReadToEnd();
            }

            List<string> skills = Utils.ParseSkillsFromHrXML(sampleXML);

            if ( skills.Count == 48 )                
                Assert.True(true);
            else
                Assert.True(false);             
        }


        [Fact]
        public void BadXML()
        {
            bool rVal = false;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "API.Tests.TestFiles.example-hrxml_bad_xml.xml";
            string sampleXML = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                sampleXML = reader.ReadToEnd();
            }

            try
            {
                List<string> skills = Utils.ParseSkillsFromHrXML(sampleXML);
            }
            catch
            {
                rVal = true;
            }   
            
            Assert.True(rVal);     
        }



    }
}

    