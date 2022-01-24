using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    /// <summary>
    /// The Blob Full Name Model
    /// </summary>
    public class AzureStorageBlobFullNameModel
    {
        public string ServiceName { get; private set; }
        public string Path { get; private set; }

        public string BlobName { get; private set; }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <param name="path">The blob folder path</param>
        /// <param name="blobName">The blob name</param>
        public AzureStorageBlobFullNameModel(string serviceName, string path, string blobName)
        {
            this.ServiceName = (string.IsNullOrEmpty(serviceName) ? throw new ArgumentNullException(nameof(serviceName)) : serviceName);
            this.Path = (string.IsNullOrEmpty(path) ? throw new ArgumentNullException(nameof(path)) : path);
            this.BlobName = (string.IsNullOrEmpty(blobName) ? throw new ArgumentNullException(nameof(blobName)) : blobName);
        }
    }
}
