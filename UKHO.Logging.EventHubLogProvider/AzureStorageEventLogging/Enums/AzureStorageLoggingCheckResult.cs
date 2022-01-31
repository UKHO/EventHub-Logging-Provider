namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums
{
    /// <summary>
    ///     The Azure Storage Logging Check Result enumeration
    /// </summary>
    public enum AzureStorageLoggingCheckResult
    {
        NoLogging = 0,
        NoLoggingWithMessageWarning = 1,
        LoggingWithMessage = 2
    }
}