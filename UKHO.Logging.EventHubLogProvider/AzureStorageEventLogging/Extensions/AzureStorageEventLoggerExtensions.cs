using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

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
        public static bool IsLongMessage(this string message, int mbs)
        {
            int messageSize = Encoding.UTF8.GetByteCount(message);
            return (messageSize >= (mbs * (1024 * 1024)));


        }

        /// <summary>
        /// Checks if azure storage logging in necessary
        /// </summary>
        /// <param name="builderModel">The builder model</param>
        /// <param name="message">The message</param>
        /// <param name="mbs">The size(MBs)</param>
        /// <returns>True if logging is necessary</returns>
        public static bool NeedsAzureStorageLogging(this AzureStorageBlobContainerBuilder builderModel, string message, int mbs)
        {


            if (builderModel != null && builderModel.AzureStorageLogProviderOptions != null && (builderModel.AzureStorageLogProviderOptions.AzureStorageLoggerEnabled) && IsLongMessage(message, mbs))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Gets a property value from a dictionary
        /// </summary>
        /// <param name="set">The dictionary</param>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public static string GetLogEntryPropertyValue(this Dictionary<string, object> set, string key)
        {
            object value;
            bool keyExists = set.TryGetValue(key, out value);

            return ((keyExists == true & value != null) ? value.ToString() : null);
        }

        /// <summary>
        /// Creates a json log entry string from an azure storage log result
        /// </summary>
        /// <param name="azureStorageLogResult">The azure log storage result</param>
        /// <param name="azureStorageLogProviderOptions">The azure log provider options</param>
        /// <param name="existingLogEntry">The azure log entry</param>
        /// <param name="jsonSettings">The json serialization settings</param>
        /// <returns>The log entry string</returns>
        public static string ToJsonLogEntryString(this AzureStorageEventLogResult azureStorageLogResult 
                                                  ,AzureStorageLogProviderOptions azureStorageLogProviderOptions 
                                                  ,LogEntry existingLogEntry
                                                  , JsonSerializerSettings jsonSettings)
        {
            string template = azureStorageLogResult.ToMessageTemplate(azureStorageLogProviderOptions);
            string logMessage = azureStorageLogResult.ToLogMessage(azureStorageLogProviderOptions, template);
            existingLogEntry.MessageTemplate = template;
            existingLogEntry.Exception = new Exception(logMessage);
            return JsonConvert.SerializeObject(existingLogEntry, jsonSettings);
        }
        /// <summary>
        /// Selected the message template based on the azure storage result and the azure storage provider options
        /// </summary>
        /// <param name="azureStorageLogResult">The azure storage result </param>
        /// <param name="azureStorageLogProviderOptions">The azure storage provider options</param>
        /// <returns></returns>
        public static string ToMessageTemplate(this AzureStorageEventLogResult azureStorageLogResult,  AzureStorageLogProviderOptions azureStorageLogProviderOptions)
        {
            return (azureStorageLogResult.IsStored ? azureStorageLogProviderOptions.SuccessfulMessageTemplate :
                azureStorageLogProviderOptions.FailedMessageTemplate);
        }
        /// <summary>
        /// Creates a log message based on the azure storage log result
        /// </summary>
        /// <param name="azureStorageLogResult">The azure storage result</param>
        /// <param name="azureStorageLogProviderOptions">The azure storage log provider options</param>
        /// <param name="template">The template</param>
        /// <returns>The azure storage log message</returns>
        public static string ToLogMessage(this AzureStorageEventLogResult azureStorageLogResult, AzureStorageLogProviderOptions azureStorageLogProviderOptions, string template)
        {
            

            StringBuilder sb = new StringBuilder(template);


            foreach (var prop in azureStorageLogResult.GetType().GetProperties())
            {
                string oldValue = String.Format("{0}{1}{2}", "{{", prop.Name, "}}");
                object value = prop.GetValue(azureStorageLogResult, null);
                string newValue = (value != null ? value.ToString() : null);
                sb.Replace(oldValue, newValue);
            }

            return sb.ToString();
        }
    }
}
