namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    /// <summary>
    ///     The azure storage event log result
    /// </summary>
    public class AzureStorageEventLogResult
    {
        /// <summary>
        ///     The default constructor
        /// </summary>
        /// <param name="reasonPhrase">The result reason phrase</param>
        /// <param name="statusCode">The http status code</param>
        /// <param name="requestId">The client request Id</param>
        /// <param name="responseId">The response Id</param>
        /// <param name="isStored">The flag that determines if the result was successful/failed</param>
        /// <param name="blobFullName">The blob full name</param>
        public AzureStorageEventLogResult(string reasonPhrase, int statusCode, string requestId, long responseId, bool isStored, string blobFullName)
        {
            BlobFullName = blobFullName;
            IsStored = isStored;
            ReasonPhrase = reasonPhrase;
            RequestId = requestId;
            ResponseId = responseId;
            StatusCode = statusCode;
        }

        public string ReasonPhrase { get; set; }
        public int StatusCode { get; set; }
        public string RequestId { get; set; }
        public long ResponseId { get; set; }
        public bool IsStored { get; set; }
        public string BlobFullName { get; set; }
    }
}