using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.Logging.EventHubLogProvider.Settings;

namespace UKHO.Logging.EventHubLogProvider
{
    public class AzureStorageEventLogger
    {
        private BlobContainerClient _containerClient;

        private AppSettings _appSettings;

        public AzureStorageEventLogger()
        {
            this._appSettings = new AppSettings();
            SetUpContainerClient();
        }

        /// <summary>
        /// Generates a path for the error message blob
        /// </summary>
        /// <param name="subscriptionName">The subscription name</param>
        /// <param name="date">The datetime</param>
        /// <returns>The blob path</returns>
        public string GeneratePathForErrorBlob(string subscriptionName, DateTime date)
        {
            return Path.Combine(subscriptionName
                                ,date.Year.ToString()
                                ,date.Month.ToString()
                                , date.Day.ToString()
                                , date.Hour.ToString()
                                , date.Minute.ToString()
                                , date.Minute.ToString());
        }

        /// <summary>
        /// Generates a new blob name
        /// </summary>
        /// <param name="name">The name of the blob</param>
        /// <param name="extension">The extension</param>
        /// <returns>The blob name(with extension)</returns>
        public string GenerateErrorBlobName(Nullable<Guid> name , string extension)
        {
            string blobName;
            if (name == null)
            {
                blobName = Guid.NewGuid().ToString().Replace("-", "_");
            }
            else
            {
                blobName = name.ToString().Replace("-", "_");
            }

            if (string.IsNullOrEmpty(extension))
            {
                blobName += String.Format("{0}{1}{2}",blobName,".","txt");
            }
            else
            {
                blobName += String.Format("{0}{1}{2}", blobName, ".", extension);
            }

            return blobName;
        }

        public void SetUpContainerClient()
        {
            Uri uri = new Uri(this._appSettings.AzureStorageContainerSasUrl);




            this._containerClient = new BlobContainerClient(uri );

            bool k = this._containerClient.CanGenerateSasUri;
        } 

        public bool StoreLogFile(AzureStorageEventModel model)
        {
             
            bool isStored = false;
            BinaryData binaryData = new BinaryData(model.Data); 
            var uploadBlobResponse = this._containerClient.UploadBlob(model.FileFullName, binaryData);

            if(uploadBlobResponse.Value != null)
            {
                isStored = true;
            }

            return isStored;
        }
    }
}
