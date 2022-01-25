using System;
using System.Collections.Generic;
using System.Text;

using Azure.Storage.Blobs;

using FakeItEasy;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    public class BlobContainerClientFakeFactory : DummyFactory<AzureStorageBlobContainerBuilder>
    {
        protected override AzureStorageBlobContainerBuilder Create()
        {
          return new AzureStorageBlobContainerBuilder(A.Dummy<AzureStorageLogProviderOptions>(),A.Fake<BlobContainerClient>());
             
        }

    }
}
