using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UKHO.Logging.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider.Settings;
using System.Net;
using System.Threading;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;

namespace UKHO.Logging.AzureStorageEventLogging
{
    public class AzureStorageEventLogger : IAzureStorageEventLogger
    {
        private BlobContainerClient _containerClient;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public AzureStorageEventLogger(BlobContainerClient containerClient)
        {
            this._containerClient = containerClient;
            this._cancellationToken = new CancellationToken();
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
        /// <param name="storageBlobFullNameModel">The blob full name model</param>
        /// <returns>The fullname of the blob (path + name)</returns>
        public string GenerateBlobFullName(AzureStorageBlobFullNameModel storageBlobFullNameModel)
        {
            return Path.Combine(storageBlobFullNameModel.ServiceName, storageBlobFullNameModel.Path, storageBlobFullNameModel.BlobName);
        }

        /// <summary>
        /// Cancels the storing operation (when possible)
        /// </summary>
        /// <returns>AzureStorageEventLogCancellationResult</returns>
        public AzureStorageEventLogCancellationResult CancelLogFileStoringOperation()
        {
            if (this._cancellationToken.CanBeCanceled == true)
            {
                try
                {
                    this.cancellationTokenSource.Cancel();
                    return AzureStorageEventLogCancellationResult.Successful;
                }
                catch (Exception ex)
                {
                    return AzureStorageEventLogCancellationResult.CancellationFailed;
                }
            }
            else
            {
                return AzureStorageEventLogCancellationResult.UnableToCancel;
            }
        }
 

        public AzureStorageEventLogResult StoreLogFile(AzureStorageEventModel model,bool withCancellation = false)
        {
             
            bool isStored = false;
            BinaryData binaryData = new BinaryData(model.Data);

            if(withCancellation == true)
            {
                this._cancellationToken = this.cancellationTokenSource.Token;
            }


            var uploadBlobResponse = this._containerClient.UploadBlob(model.FileFullName, binaryData, this._cancellationToken);
 
            if (uploadBlobResponse.Value != null )
            {
                if (uploadBlobResponse.GetRawResponse().Status == (int)HttpStatusCode.Created 
                    & uploadBlobResponse.GetRawResponse().ReasonPhrase == HttpStatusCode.Created.ToString())
                {
                    isStored = true;
                }
               
            }

            return new AzureStorageEventLogResult(uploadBlobResponse.GetRawResponse().ReasonPhrase, uploadBlobResponse.GetRawResponse().Status,
                                                  uploadBlobResponse.GetRawResponse().ClientRequestId, uploadBlobResponse.Value.BlobSequenceNumber,isStored,model.FileFullName );
        }


        public async Task<AzureStorageEventLogResult> StoreLogFileAsync(AzureStorageEventModel model, bool withCancellation = false)
        {

            bool isStored = false;
            BinaryData binaryData = new BinaryData(model.Data);

            if (withCancellation == true)
            {
                this._cancellationToken = this.cancellationTokenSource.Token;
            }


            var uploadBlobResponse = await this._containerClient.UploadBlobAsync(model.FileFullName, binaryData, this._cancellationToken);

            if (uploadBlobResponse.Value != null)
            {
                if (uploadBlobResponse.GetRawResponse().Status == (int)HttpStatusCode.Created
                    & uploadBlobResponse.GetRawResponse().ReasonPhrase == HttpStatusCode.Created.ToString())
                {
                    isStored = true;
                }

            }

            return new AzureStorageEventLogResult(uploadBlobResponse.GetRawResponse().ReasonPhrase, uploadBlobResponse.GetRawResponse().Status,
                                                  uploadBlobResponse.GetRawResponse().ClientRequestId, uploadBlobResponse.Value.BlobSequenceNumber, isStored, model.FileFullName);
        }
    }
}
