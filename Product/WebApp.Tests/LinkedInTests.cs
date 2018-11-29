using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Moq;
using System;
using UpDiddy.Helpers.RewriteRules;
using Xunit;

namespace WebApp.Tests
{
    public class LinkedInTests
    {
        [Fact]
        public void SyncViewComponent_GetLinkedInRequestAuthCodeUrl_IsValid()
        {
            /* todo: we need to write unit tests that cover functionality within the WebApp, but this presents the following challenges
             * - how to mock the API? use shims, Moq, something else?
             * - how to mock 3rd party dependencies in our code (e.g. ADB2C, KeyVault, etc)?
             * - do we need to rewrite our classes/methods so that unit tests can be written for it?
             */

            // arrange
            var mockRewriteContext = new Mock<RewriteContext>();
            HostString host = new HostString("careercircle.com");
            // the following line generates this exception - System.NotSupportedException: 'Invalid setup on a non-virtual (overridable in VB) member: mock => mock.HttpContext'
            //mockRewriteContext.SetupGet(x => x.HttpContext.Request.Host).Returns(host);

            // todo: also need to mock Scheme, PathBase, Path, QueryString
            //RedirectWwwRule redirectWwwRule = new RedirectWwwRule();

            // act
            //redirectWwwRule.ApplyRule(mockRewriteContext.Object);

            // assert
            // todo: inspect the values of response.StatusCode, response.Headers, context.Result

            // make happy noises
            Assert.True(true);
        }
    }
}
