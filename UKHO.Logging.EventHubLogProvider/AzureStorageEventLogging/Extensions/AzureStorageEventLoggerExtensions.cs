using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  System.IO;
using System.Threading.Tasks;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions
{
    public static class AzureStorageEventLoggerExtensions
    {
        /// <summary>
        /// Determines is a string is greater or equal to the  size
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="mbs">The size in MBs</param>
        /// <returns>True/False</returns>
        public static bool IsLongMessage(this string message,int mbs)
        {
            int messageSize = Encoding.UTF8.GetByteCount(message);

            return (messageSize >= (mbs * (1024^2)));
 

        }

        public static bool NeedsAzureStorageLogging(this AzureStorageBlobContainerBuilder builderModel,string message,int mbs)
        {
            

            if( builderModel.AzureStorageLogProviderOptions != null && (builderModel.AzureStorageLogProviderOptions.AzureStorageLoggerEnabled) &&  IsLongMessage(message, mbs))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static string GetLogEntryPropertyValue(this Dictionary<string,object> set,string key)
        {
            object value;
            bool keyExists = set.TryGetValue("_Service", out value);

            return ((keyExists == true & value != null ) ? value.ToString() : null );
        }
    }
}
