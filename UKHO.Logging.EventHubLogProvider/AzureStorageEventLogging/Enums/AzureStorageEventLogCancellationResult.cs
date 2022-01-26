namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums
{
    /// <summary>
    ///     The Cancellation Result enumeration
    /// </summary>
    public enum AzureStorageEventLogCancellationResult
    {
        Successful = 0,
        UnableToCancel = 1,
        CancellationFailed = 2
    }
}