using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models
{
    public class AzureStorageLogProviderOptions : IAzureStorageLogProviderOptions
    {
        public string AzureStorageContainerSasUrlString { get; private set; }
        public bool AzureStorageLoggerEnabled { get; private set; }

        public Uri AzureStorageContainerSasUrl { get; private set; }

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
 
            this.AzureStorageContainerSasUrl = this.ValidateSasUrl(azureStorageContainerSasUrlString);

        }

        /// <summary>
        ///  Validates and converts a sas url string into a url model
        /// </summary>
        /// <param name="url">The url (string)</param>
        /// <returns>A url model</returns>
        private Uri ValidateSasUrl(string url)
        {
            Uri uri;
            bool isValid =  Uri.TryCreate(url, UriKind.RelativeOrAbsolute,out uri);

            if(isValid == false)
            {
                throw new UriFormatException("Invalid sas url.");
            }

            return uri;
        }


    }
}
