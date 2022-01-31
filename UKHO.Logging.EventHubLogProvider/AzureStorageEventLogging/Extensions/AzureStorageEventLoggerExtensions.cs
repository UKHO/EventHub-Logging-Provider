﻿using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Newtonsoft.Json;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions
{
    /// <summary>
    ///     The Azure storage event logger extensions
    /// </summary>
    public static class AzureStorageEventLoggerExtensions
    {
        /// <summary>
        ///     Determines is a string is greater or equal to the  size
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="mbs">The size in MBs</param>
        /// <returns>True/False</returns>
        public static bool IsLongMessage(this string message, int mbs)
        {
            var messageSize = Encoding.UTF8.GetByteCount(message);
            return messageSize >= mbs * 1024 * 1024;
        }

        /// <summary>
        ///     Checks if azure storage logging in necessary
        /// </summary>
        /// <param name="builderModel">The builder model</param>
        /// <param name="message">The message</param>
        /// <param name="mbs">The size(MBs)</param>
        /// <returns>AzureStorageLoggingCheckResult</returns>
        public static AzureStorageLoggingCheckResult NeedsAzureStorageLogging(this AzureStorageBlobContainerBuilder builderModel,
                                                    string message,
                                                    int mbs)
        {

            bool isLongMessage = IsLongMessage(message, mbs);

            if (isLongMessage == true)
            {
                if (builderModel != null && builderModel.AzureStorageLogProviderOptions != null &&
                    builderModel.AzureStorageLogProviderOptions.AzureStorageLoggerEnabled)
                {
                    return AzureStorageLoggingCheckResult.LoggingWithMessage;
                }
                else
                {
                    return AzureStorageLoggingCheckResult.NoLoggingWithMessageWarning;
                }
            }
            else
            {
                return AzureStorageLoggingCheckResult.NoLogging;
            }

        }

        public static string ToLongMessageWarning(this LogEntry logEntry,JsonSerializerSettings serializerSettings)
        {
            string template = $"A log over 1MB was submitted with a message of {logEntry.MessageTemplate}. Please enable the Azure Storage Event Logging feature to store details of oversize logs.";

            logEntry.Exception = new Exception(template);

            return JsonConvert.SerializeObject(logEntry,serializerSettings);
        }

        /// <summary>
        ///     Gets a property value from a dictionary
        /// </summary>
        /// <param name="set">The dictionary</param>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        public static string GetLogEntryPropertyValue(this Dictionary<string, object> set, string key)
        { 
            var keyExists = set.TryGetValue(key, out object value);

            return keyExists & (value != null) ? value.ToString() : null;
        }

        /// <summary>
        ///     Creates a json log entry string from an azure storage log result
        /// </summary>
        /// <param name="azureStorageLogResult">The azure log storage result</param>
        /// <param name="azureStorageLogProviderOptions">The azure log provider options</param>
        /// <param name="existingLogEntry">The azure log entry</param>
        /// <param name="jsonSettings">The json serialization settings</param>
        /// <returns>The log entry string</returns>
        public static string ToJsonLogEntryString(this AzureStorageEventLogResult azureStorageLogResult,
                                                  AzureStorageLogProviderOptions azureStorageLogProviderOptions,
                                                  LogEntry existingLogEntry,
                                                  JsonSerializerSettings jsonSettings)
        {
            var template = azureStorageLogResult.ToMessageTemplate(azureStorageLogProviderOptions);
            var logMessage = azureStorageLogResult.ToLogMessage(azureStorageLogProviderOptions, template);
            existingLogEntry.MessageTemplate = template;
            existingLogEntry.Exception = new Exception(logMessage);
            existingLogEntry.LogProperties = null;
            return JsonConvert.SerializeObject(existingLogEntry, jsonSettings);
        }

        /// <summary>
        ///     Selected the message template based on the azure storage result and the azure storage provider options
        /// </summary>
        /// <param name="azureStorageLogResult">The azure storage result </param>
        /// <param name="azureStorageLogProviderOptions">The azure storage provider options</param>
        /// <returns></returns>
        public static string ToMessageTemplate(this AzureStorageEventLogResult azureStorageLogResult,
                                               AzureStorageLogProviderOptions azureStorageLogProviderOptions)
        {
            return azureStorageLogResult.IsStored
                ? azureStorageLogProviderOptions.SuccessfulMessageTemplate
                : azureStorageLogProviderOptions.FailedMessageTemplate;
        }

        /// <summary>
        ///     Creates a log message based on the azure storage log result
        /// </summary>
        /// <param name="azureStorageLogResult">The azure storage result</param>
        /// <param name="azureStorageLogProviderOptions">The azure storage log provider options</param>
        /// <param name="template">The template</param>
        /// <returns>The azure storage log message</returns>
        public static string ToLogMessage(this AzureStorageEventLogResult azureStorageLogResult,
                                          AzureStorageLogProviderOptions azureStorageLogProviderOptions,
                                          string template)
        {
            var sb = new StringBuilder(template);

            foreach (var prop in azureStorageLogResult.GetType().GetProperties())
            {
                var oldValue = string.Format("{0}{1}{2}", "{{", prop.Name, "}}");
                var value = prop.GetValue(azureStorageLogResult, null);
                var newValue = value != null ? value.ToString() : null;
                sb.Replace(oldValue, newValue);
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Gets the file size from the headers
        /// </summary>
        /// <param name="response">The azure response</param>
        /// <param name="fileSize">The data size</param>
        /// <returns>The data size</returns>
        public static int GetFileSize(this Response response, int fileSize)
        {
            if (response == null || response.ContentStream == null || response.ContentStream.CanRead == false)
                return fileSize;

            response.Headers.TryGetValue("Content-Length", out var contentLength);

            if (contentLength == null)
                return fileSize;

            return Convert.ToInt32(contentLength) == 0 ? fileSize : Convert.ToInt32(contentLength);
        }

        /// <summary>
        ///     Gets the modified date for the uploaded file
        /// </summary>
        /// <param name="response">The azure response</param>
        /// <returns>The blob modified date</returns>
        public static DateTime? GetModifiedDate(this Response response)
        {
            if (response == null || response.ContentStream == null || response.ContentStream.CanRead == false)
                return null;

            response.Headers.TryGetValue("Date", out var date);

            return date == null ? null : (DateTime?)DateTime.Parse(date);
        }
    }
}