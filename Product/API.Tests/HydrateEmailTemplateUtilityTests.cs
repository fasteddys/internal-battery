using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using Xunit;

namespace API.Tests
{
    public class HydrateEmailTemplateUtilityTests
    {
        [Fact]
        public void HydrateTemplate()
        {
            // Arrange
            var template = "Value: {{Profile.FirstName}}";
            var profile = new Profile { FirstName = "Some user" };
            var recruiter = new Recruiter();
            var expected = "Value: Some user";

            // Act
            var actual = HydrateEmailTemplateUtility.HydrateEmailTemplates(template, recruiter, new List<Profile> { profile });

            // Assert
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal(expected, actual[0].Value);
        }

        [Fact]
        public void HydrateNestedProperties()
        {
            // Arrange
            var template = "Based in: {{Profile.City.Name}}, {{Profile.City.State.Name}}";
            var profile = new Profile
            {
                City = new City
                {
                    Name = "Baltimore",
                    State = new State
                    {
                        Name = "Md"
                    }
                }
            };
            var recruiter = new Recruiter();
            var expected = "Based in: Baltimore, Md";

            // Act
            var actual = HydrateEmailTemplateUtility.HydrateEmailTemplates(template, recruiter, new List<Profile> { profile });

            // Assert
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal(expected, actual[0].Value);
        }

        [Fact]
        public void CanFallback()
        {
            // Arrange
            var template = "value: {{Recruiter.NoSuchProp,Fallback Value}}";
            var profile = new Profile();
            var recruiter = new Recruiter();
            var expected = "value: Fallback Value";

            // Act
            var actual = HydrateEmailTemplateUtility.HydrateEmailTemplates(template, recruiter, new List<Profile> { profile });

            // Assert
            Assert.NotNull(actual);
            Assert.Single(actual);
            Assert.Equal(expected, actual[0].Value);
        }

    }
}
