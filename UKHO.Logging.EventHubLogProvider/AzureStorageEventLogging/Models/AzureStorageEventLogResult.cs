using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    public class AzureStorageEventLogResult
    {
        public string ReasonPhrase { get; set; }
        public int StatusCode { get; set; }
        public string RequestId { get; set; }
        public long ResponseId { get; set; }

        public bool IsStored { get; set; }

        public string BlobFullName { get; set; }

        public AzureStorageEventLogResult(string reasonPhrase, int statusCode , string requestId, long responseId, bool isStored, string blobFullName)
        {
            this.BlobFullName = blobFullName;
            this.IsStored = isStored;
            this.ReasonPhrase = reasonPhrase;
            this.RequestId = requestId;
            this.ResponseId = responseId;
            this.StatusCode = statusCode; 
        }
    }
}
