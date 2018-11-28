using System;
using Xunit;

namespace UpDiddyLib.Tests
{
    public class UtilsTests
    {
        /* todo: set up "theory" tests in this format?
        [Theory]
        [InlineData("")]
        public void RemoveQueryStringFromUrl_ReturnsValidUrl(string url)
        {
            Assert.True(true);
        }
        */

        [Fact]
        public void RemoveQuerStringFromUrl_NoParameters()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public void RemoveQuerStringFromUrl_MultipleQuestionMarks()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/?q=somevalue?");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }

        [Fact]
        public void RemoveQuerStringFromUrl_QuestionMarkWithoutParameters()
        {
            string modifiedUrl = UpDiddyLib.Helpers.Utils.RemoveQueryStringFromUrl("https://www.google.com/?");
            Assert.True(Uri.IsWellFormedUriString(modifiedUrl, UriKind.RelativeOrAbsolute));
        }
    }
}