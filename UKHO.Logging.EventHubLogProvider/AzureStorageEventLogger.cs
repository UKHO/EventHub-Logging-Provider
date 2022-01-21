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
        /// Generates the service name for the container subfolder
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <param name="environment">The environment</param>
        /// <returns>the service name(string)</returns>
        public string GenerateServiceName(string serviceName,string environment)
        {
            return string.Format("{0} - {1}", serviceName, environment);
        }

        /// <summary>
        /// Generates a path for the error message blob
        /// </summary>
        /// <param name="date">The datetime</param>
        /// <returns>The blob path</returns>
        public string GeneratePathForErrorBlob(DateTime date)
        {
            return Path.Combine( date.Year.ToString()
                                ,date.Month.ToString()
                                , date.Day.ToString()
                                , date.Hour.ToString()
                                , date.Minute.ToString()
                                , date.Second.ToString());
        }

        /// <summary>
        /// Generates a new blob name
        /// </summary>
        /// <param name="name">The name of the blob</param>
        /// <param name="extension">The extension</param>
        /// <returns>The blob name(with extension)</returns>
        public string GenerateErrorBlobName(Nullable<Guid> name = null, string extension = null)
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
                blobName += String.Format("{0}{1}",".","txt");
            }
            else
            {
                blobName += String.Format("{0}{1}", ".", extension);
            }

            return blobName;
        }

        /// <summary>
        /// Generates the full name for the blob
        /// </summary>
        /// <param name="serviceName">The service name (ess etc)</param>
        /// <param name="path">The path (date based)</param>
        /// <param name="blobName">The blob name</param>
        /// <returns>The fullname of the blob (path + name)</returns>
        public string GenerateBlobFullName(string serviceName, string path, string blobName)
        {
            return Path.Combine(serviceName,path,blobName);
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
