using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;
using UKHO.Logging.EventHubLogProvider.Settings;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    public class AzureStorageLogProviderOptions : IAzureStorageLogProviderOptions
    {
        public string AzureStorageContainerSasUrlString { get; private set; }
        public bool AzureStorageLoggerEnabled { get; private set; }

        public Uri AzureStorageContainerSasUrl { get; private set; }

        public string SuccessfulMessageTemplate { get; private set; }
        public string FailedMessageTemplate { get; private set; }


        /// <summary>
        /// The Options model for the Azure Storage Log Provider
        /// </summary>
        /// <param name="azureStorageContainerSasUrlString">The sas url</param>
        /// <param name="azureStorageLoggerEnabled">The azure storage enabled flag</param>
        public AzureStorageLogProviderOptions(string azureStorageContainerSasUrlString, bool azureStorageLoggerEnabled)
        {
            this.AzureStorageLoggerEnabled = azureStorageLoggerEnabled;
            this.AzureStorageContainerSasUrlString = azureStorageContainerSasUrlString;


            if(this.AzureStorageLoggerEnabled == true & String.IsNullOrEmpty(this.AzureStorageContainerSasUrlString))
            {
                throw new NullReferenceException("The Azure storage container sas url cannot be null or empty when Azure storage option is set to enabled");
            }
  
            string f= AppSettings.GetSetting(String.Format("{0}.{1}", "AzureStorage", nameof(SuccessfulMessageTemplate)));
            this.SuccessfulMessageTemplate = AppSettings.GetSetting(String.Format("{0}.{1}","AzureStorage",nameof(SuccessfulMessageTemplate)));
            this.FailedMessageTemplate = AppSettings.GetSetting(String.Format("{0}.{1}", "AzureStorage", nameof(FailedMessageTemplate)));
        }


        /// <summary>
        /// The Options model for the Azure Storage Log Provider
        /// </summary>
        /// <param name="azureStorageContainerSasUrlString">The sas url</param>
        /// <param name="azureStorageLoggerEnabled">The azure storage enabled flag</param>
        public AzureStorageLogProviderOptions(string azureStorageContainerSasUrlString, bool azureStorageLoggerEnabled, string successfulMessageTemplate, string failedMessageTemplate)
        {
            this.AzureStorageLoggerEnabled = azureStorageLoggerEnabled;
            this.AzureStorageContainerSasUrlString = azureStorageContainerSasUrlString;


            if (this.AzureStorageLoggerEnabled == true & String.IsNullOrEmpty(this.AzureStorageContainerSasUrlString))
            {
                throw new NullReferenceException("The Azure storage container sas url cannot be null or empty when Azure storage option is set to enabled");
            }

            this.AzureStorageContainerSasUrl = this.ValidateSasUrl(azureStorageContainerSasUrlString);

            this.SuccessfulMessageTemplate = ( (string.IsNullOrEmpty(successfulMessageTemplate) | string.IsNullOrWhiteSpace(successfulMessageTemplate)) ?
                throw new NullReferenceException("The successful message template cannot be null.empty or whitespace") : successfulMessageTemplate );
            this.FailedMessageTemplate = ((string.IsNullOrEmpty(failedMessageTemplate) | string.IsNullOrWhiteSpace(failedMessageTemplate)) ?
                throw new NullReferenceException("The failed message template cannot be null.empty or whitespace") : failedMessageTemplate);

        }

        /// <summary>
        ///  Validates and converts a sas url string into a url model
        /// </summary>
        /// <param name="url">The url (string)</param>
        /// <returns>A url model</returns>
        private Uri ValidateSasUrl(string url)
        {
            Uri uri=null;
            bool isValid =  (Uri.TryCreate(url, UriKind.Absolute,out uri) && Uri.IsWellFormedUriString(url ,UriKind.Absolute) );
             
            if(isValid == false)
            {
                throw new UriFormatException("Invalid sas url.");
            }

            return uri;
        }


    }
}
