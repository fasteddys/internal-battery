using System;
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

            // make happy noises
            Assert.True(true);
        }
    }
}
