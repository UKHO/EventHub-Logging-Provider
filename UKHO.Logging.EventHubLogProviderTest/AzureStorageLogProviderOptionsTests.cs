using System;
using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     Tests for the azure storage log provider
    /// </summary>
    [TestFixture]
    public class AzureStorageLogProviderOptionsTests
    {
        /// <summary>
        ///     Test for the method that validates the url string (When the url string is an invalid url string)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_InvalidUrl()
        {
            var url = "-test";

            Assert.Throws(typeof(UriFormatException), () => new AzureStorageLogProviderOptions(url, true), "Invalid sas url.");
        }

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is null)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_NullUrl()
        {
            string url = null;
            Assert.Throws(typeof(NullReferenceException), () => new AzureStorageLogProviderOptions(url, true));
        }

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is a valid url string)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_ValidUrl()
        {
            var url = "https://test/test/test/";
            var result = new AzureStorageLogProviderOptions(url, true);
            Assert.IsNotNull(result.AzureStorageContainerSasUrl);
            Assert.AreEqual(result.AzureStorageContainerSasUrl.AbsoluteUri, url);
        }
    }
}