using System;
using System.Collections.Generic;
using System.Text;

using FakeItEasy;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class AzureStorageLogProviderOptionsTests
    {
        
 
        [Test]
        public void Test_ValidateSasUrl_ValidUrl()
        {
            string url = "https://test/test/test/";
            var result = new AzureStorageLogProviderOptions(url, true);
            Assert.IsNotNull(result.AzureStorageContainerSasUrl);
            Assert.AreEqual(result.AzureStorageContainerSasUrl.AbsoluteUri, url);
        }

        [Test]
        public void Test_ValidateSasUrl_InvalidUrl()
        {
            string url = "-test";
             
            Assert.Throws(typeof(UriFormatException), ()=> new AzureStorageLogProviderOptions(url, true), "Invalid sas url."); 
        }

        [Test]
        public void Test_ValidateSasUrl_NullUrl()
        {
            string url =null; 
            Assert.Throws(typeof(System.NullReferenceException), () => new AzureStorageLogProviderOptions(url, true));
        }
    }
}
