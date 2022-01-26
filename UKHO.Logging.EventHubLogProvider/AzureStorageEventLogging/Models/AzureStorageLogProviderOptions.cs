using System;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;
using UKHO.Logging.EventHubLogProvider.Settings;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    /// <summary>
    ///     The storage provider options model
    /// </summary>
    public class AzureStorageLogProviderOptions : IAzureStorageLogProviderOptions
    {
        /// <summary>
        ///     The Options model for the Azure Storage Log Provider
        /// </summary>
        /// <param name="azureStorageContainerSasUrlString">The sas url</param>
        /// <param name="azureStorageLoggerEnabled">The azure storage enabled flag</param>
        public AzureStorageLogProviderOptions(string azureStorageContainerSasUrlString, bool azureStorageLoggerEnabled)
        {
            AzureStorageLoggerEnabled = azureStorageLoggerEnabled;
            AzureStorageContainerSasUrlString = azureStorageContainerSasUrlString;

            if (AzureStorageLoggerEnabled)
            {
                if (string.IsNullOrEmpty(AzureStorageContainerSasUrlString))
                    throw new NullReferenceException("The Azure storage container sas url cannot be null or empty when Azure storage option is set to enabled");
                AzureStorageContainerSasUrl = ValidateSasUrl(azureStorageContainerSasUrlString);
            }

            SuccessfulMessageTemplate = AppSettings.GetSetting(string.Format("{0}.{1}", "AzureStorage", nameof(SuccessfulMessageTemplate)));
            FailedMessageTemplate = AppSettings.GetSetting(string.Format("{0}.{1}", "AzureStorage", nameof(FailedMessageTemplate)));
        }

        /// <summary>
        ///     The Options model for the Azure Storage Log Provider
        /// </summary>
        /// <param name="azureStorageContainerSasUrlString">The sas url</param>
        /// <param name="azureStorageLoggerEnabled">The azure storage enabled flag</param>
        public AzureStorageLogProviderOptions(string azureStorageContainerSasUrlString,
                                              bool azureStorageLoggerEnabled,
                                              string successfulMessageTemplate,
                                              string failedMessageTemplate)
        {
            AzureStorageLoggerEnabled = azureStorageLoggerEnabled;
            AzureStorageContainerSasUrlString = azureStorageContainerSasUrlString;

            if (AzureStorageLoggerEnabled)
            {
                if (string.IsNullOrEmpty(AzureStorageContainerSasUrlString))
                    throw new NullReferenceException("The Azure storage container sas url cannot be null or empty when Azure storage option is set to enabled");
                AzureStorageContainerSasUrl = ValidateSasUrl(azureStorageContainerSasUrlString);
            }

            SuccessfulMessageTemplate = string.IsNullOrEmpty(successfulMessageTemplate) | string.IsNullOrWhiteSpace(successfulMessageTemplate)
                ? throw new NullReferenceException("The successful message template cannot be null.empty or whitespace")
                : successfulMessageTemplate;
            FailedMessageTemplate = string.IsNullOrEmpty(failedMessageTemplate) | string.IsNullOrWhiteSpace(failedMessageTemplate)
                ? throw new NullReferenceException("The failed message template cannot be null.empty or whitespace")
                : failedMessageTemplate;
        }

        /// <summary>
        ///     The azure storage sas url string
        /// </summary>
        public string AzureStorageContainerSasUrlString { get; }

        /// <summary>
        ///     Enable the azure storage functionality
        /// </summary>
        public bool AzureStorageLoggerEnabled { get; }

        /// <summary>
        ///     The azure storage uri model
        /// </summary>
        public Uri AzureStorageContainerSasUrl { get; }

        /// <summary>
        ///     The azure storage result successful template
        /// </summary>
        public string SuccessfulMessageTemplate { get; private set; }

        /// <summary>
        ///     The azure storage result failed template
        /// </summary>
        public string FailedMessageTemplate { get; private set; }

        /// <summary>
        ///     Validates and converts a sas url string into a url model
        /// </summary>
        /// <param name="url">The url (string)</param>
        /// <returns>A url model</returns>
        private Uri ValidateSasUrl(string url)
        {
            Uri uri = null;
            var isValid = Uri.TryCreate(url, UriKind.Absolute, out uri) && Uri.IsWellFormedUriString(url, UriKind.Absolute);

            if (isValid == false)
                throw new UriFormatException("Invalid sas url.");

            return uri;
        }
    }
}