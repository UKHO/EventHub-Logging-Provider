using System;

namespace UKHO.Logging.EventHubLogProvider
{
    public class AzureStorageEventModel
    {
        public string Data { get; private set; }
        public string FileFullName { get; private set; } 

        public AzureStorageEventModel(string fileFullName, string data)
        {
             
            this.FileFullName = (string.IsNullOrEmpty(fileFullName) ? throw new NullReferenceException(nameof(this.FileFullName)) : fileFullName);
            this.Data = (string.IsNullOrEmpty(data) ? throw new NullReferenceException(nameof(this.Data)) : data);
        }
    }
}