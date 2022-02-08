using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProviderTest.Factories;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     Tests for the azure blob container builder
    /// </summary>
    [TestFixture]
    public class AzureStorageBlobContainerBuilderTests
    {
        private readonly ResourcesFactory resourcesFactory = new ResourcesFactory();

        /// <summary>
        ///     Test for the Build with null options
        /// </summary>
        [Test]
        public void Test_Build_WithNullOptions()
        {
            var azureOptionsModel = new AzureStorageBlobContainerBuilder(null);
            azureOptionsModel.Build();
            Assert.IsNull(azureOptionsModel.AzureStorageLogProviderOptions);
            Assert.IsNull(azureOptionsModel.BlobContainerClient);
        }

        /// <summary>
        ///     Test for the Build with options
        /// </summary>
        [Test]
        public void Test_Build_WithOptions()
        {
            var azureOptionsModel =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
            azureOptionsModel.Build();
            Assert.NotNull(azureOptionsModel.AzureStorageLogProviderOptions);
            Assert.NotNull(azureOptionsModel.BlobContainerClient);
        }
    }
}