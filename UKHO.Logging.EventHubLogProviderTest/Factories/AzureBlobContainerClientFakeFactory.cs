using Azure.Storage.Blobs;
using FakeItEasy;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    /// <summary>
    ///     Dummy factory for the blob container builder
    /// </summary>
    public class BlobContainerClientFakeFactory : DummyFactory<AzureStorageBlobContainerBuilder>
    {
        /// <summary>
        ///     Creates and returns a blob container builder model(fake)
        /// </summary>
        /// <returns>a blob container builder model(fake)</returns>
        protected override AzureStorageBlobContainerBuilder Create()
        {
            return new AzureStorageBlobContainerBuilder(A.Dummy<AzureStorageLogProviderOptions>(), A.Fake<BlobContainerClient>());
        }
    }
}