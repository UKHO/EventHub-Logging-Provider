using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    public class AzureStorageBlobContainerBuilder
    {
        public AzureStorageLogProviderOptions AzureStorageLogProviderOptions { get; private set; }
        public BlobContainerClient BlobContainerClient { get; private set; }

        public AzureStorageBlobContainerBuilder(AzureStorageLogProviderOptions azureStorageLogProviderOptions)
        {
            this.AzureStorageLogProviderOptions = azureStorageLogProviderOptions;
             
        }
        public AzureStorageBlobContainerBuilder(AzureStorageLogProviderOptions azureStorageLogProviderOptions, BlobContainerClient blobContainerClient)
        {
            this.AzureStorageLogProviderOptions = azureStorageLogProviderOptions;
            this.BlobContainerClient = blobContainerClient;
        }

        public void Build()
        {
            if(this.AzureStorageLogProviderOptions != null)
            {
                this.BlobContainerClient = new BlobContainerClient(this.AzureStorageLogProviderOptions.AzureStorageContainerSasUrl);
            }
        }
 
    }
}
