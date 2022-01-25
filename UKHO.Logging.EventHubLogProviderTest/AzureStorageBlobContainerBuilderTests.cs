using System;
using System.Collections.Generic;
using System.Text;

using FakeItEasy;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class AzureStorageBlobContainerBuilderTests
    {
        [Test]
        public void Test_Build_WithNullOptions()
        {
             
            var azureOptionsModel = new AzureStorageBlobContainerBuilder(null);
            azureOptionsModel.Build();
            Assert.IsNull(azureOptionsModel.AzureStorageLogProviderOptions);
            Assert.IsNull(azureOptionsModel.BlobContainerClient);
        }

        [Test]
        public void Test_Build_WithOptions()
        {

            var azureOptionsModel = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",true));
            azureOptionsModel.Build();
            Assert.NotNull(azureOptionsModel.AzureStorageLogProviderOptions);
            Assert.NotNull(azureOptionsModel.BlobContainerClient);
        }
    }
}
