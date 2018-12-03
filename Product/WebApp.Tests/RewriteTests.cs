using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using UpDiddy.Helpers.RewriteRules;
using Xunit;

namespace WebApp.Tests
{
    public class RewriteTests
    {
        /// <summary>
        /// Constructs a fake <see cref="HttpContext"/> that can be used to test rewrite rules.
        /// </summary>
        /// <param name="scheme">The protocol used in the request (http or https)</param>
        /// <param name="host">The host portion of the http request</param>
        /// <param name="path">The path portion of the http request</param>
        /// <param name="queryString">The query string portion of the http request</param>
        /// <returns>The faked HttpContext</returns>
        public static HttpContext GetMockedHttpContext(string scheme, string host, string path, string queryString)
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(r => r.Host).Returns(new HostString(host));
            httpRequest.Setup(r => r.Scheme).Returns(scheme);
            httpRequest.Setup(r => r.PathBase).Returns(string.Empty);
            httpRequest.Setup(r => r.Path).Returns(path);
            httpRequest.Setup(r => r.QueryString).Returns(new QueryString(queryString));

            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty(r => r.StatusCode);
            var headers = new HeaderDictionary();
            headers.Add(HeaderNames.Location, string.Empty);
            httpResponse.SetupGet(r => r.Headers).Returns(headers);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns(httpRequest.Object);
            httpContext.Setup(c => c.Response).Returns(httpResponse.Object);

            return httpContext.Object;
        }

        /// <summary>
        /// Verifies that requests which do not begin with "www." are redirected with "www." at the beginning of the url
        /// </summary>
        [Fact]
        public void Rewrite_rule_redirects_to_www()
        {
            // Arrange
            var rewriteContext = new Mock<RewriteContext>();
            rewriteContext.Object.HttpContext = RewriteTests.GetMockedHttpContext("https", "careercircle.com", "/Topic/Cyber-Security", string.Empty);
            RedirectWwwRule redirectWwwRule = new RedirectWwwRule();

            // Act
            redirectWwwRule.ApplyRule(rewriteContext.Object);

            // Assert
            Assert.Contains("//www.", rewriteContext.Object.HttpContext.Response.Headers[HeaderNames.Location].ToArray()[0]);
            Assert.Equal(301, rewriteContext.Object.HttpContext.Response.StatusCode);
            Assert.Equal(RuleResult.EndResponse, rewriteContext.Object.Result);
        }

        /// <summary>
        /// Verifies that requests which do begin with "www." are unaffected by the rewrite rule
        /// </summary>
        [Fact]
        public void Rewrite_rule_ignored_when_www_exists()
        {
            // Arrange
            var rewriteContext = new Mock<RewriteContext>();
            rewriteContext.Object.HttpContext = RewriteTests.GetMockedHttpContext("https", "www.careercircle.com", "/Topic/Cyber-Security", string.Empty);
            RedirectWwwRule redirectWwwRule = new RedirectWwwRule();

            // Act
            redirectWwwRule.ApplyRule(rewriteContext.Object);

            // Assert
            Assert.Equal(RuleResult.ContinueRules, rewriteContext.Object.Result);
        }
    }
}
