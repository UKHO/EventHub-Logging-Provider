using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     Tests for the Azure storage event logger extensions
    /// </summary>
    [TestFixture]
    public class AzureStorageEventLoggerExtensionsTests
    {
        /// <summary>
        ///     Generates a test string message
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns>A message</returns>
        private string GenerateTestMessage(int size)
        {
            var charsPool = "ABCDEFGHJKLMNOPQRSTVUWXYZ1234567890";
            var charsArray = new char[size];

            var rand = new Random();

            for (var c = 0; c < charsArray.Length; c++)
                charsArray[c] = charsPool[rand.Next(charsPool.Length)];

            return new string(charsArray);
        }

        /// <summary>
        ///     Test for the IsLongMessage extension (When message is 1 MB)
        /// </summary>
        [Test]
        public void Test_IsLongMessage_WithMessageEqualToMbs()
        {
            var size = 1;
            var message = GenerateTestMessage(1024 * 1024);
            var result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the IsLongMessage extension (When message is greater than 1 MB)
        /// </summary>
        [Test]
        public void Test_IsLongMessage_WithMessageGreaterThanMbs()
        {
            var size = 1;
            var message = GenerateTestMessage(1024 * 1025);
            var result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the IsLongMessage extension (When message is less than 1 MB)
        /// </summary>
        [Test]
        public void Test_IsLongMessage_WithMessageLessThanMbs()
        {
            var size = 1;
            var message = GenerateTestMessage(1024 * 512);
            var result = message.IsLongMessage(size);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  logger enabled flag is false)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledFalse()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", false));
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  logger enabled flag is true)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledTrue()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  log provider options are not null)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNotNull()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  log provider options are null)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNull()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(null);
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When builder model is not null)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNotNull()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When builder model is null)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = null;
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  the size of the message is equal to the defined size)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageEqualToDefinedSize()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 1024);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  the size of the message is greater than the defined size)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageGreaterThanDefinedSize()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 1025);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Test for the azure storage needs logging extension (When  the size of the message is less than the defined size)
        /// </summary>
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageLessThanDefinedSize()
        {
            var azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            var message = GenerateTestMessage(1024 * 512);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Tests for the LogEntry Get Property Value (When key exists)
        /// </summary>
        [Test]
        public void TestGetLogEntryPropertyValueKeyExists()
        {
            var set = new Dictionary<string, object>();
            set.Add("test_key_exists", "test_value");
            var result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNotNull(result);
            Assert.AreEqual("test_value", result);
        }

        /// <summary>
        ///     Tests for the LogEntry Get Property Value (When key does not exist)
        /// </summary>
        [Test]
        public void TestGetLogEntryPropertyValueKeyNotExists()
        {
            var set = new Dictionary<string, object>();
            set.Add("test_key__not_exists", "test_value");
            var result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNull(result);
        }

        /// <summary>
        ///     Test for the extension method that creates and returns the log entry as a json string
        /// </summary>
        [Test]
        public void TestToJsonLogEntryString()
        {
            var dt = DateTime.UtcNow;
            var requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);
            var azureStorageLogProviderOptions = new AzureStorageLogProviderOptions("https://test.com", true);
            var logEntry = new LogEntry
                           {
                               Exception = new Exception(""),
                               Level = "Warning",
                               MessageTemplate = "Log Serialization failed with exception",
                               Timestamp = dt,
                               EventId = new EventId(7437)
                           };
            var jsonSettings = new JsonSerializerSettings
                               {
                                   Formatting = Formatting.Indented,
                                   ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                   ContractResolver = new NullPropertyResolver()
                               };
            var result = azureStorageEventLogResult.ToJsonLogEntryString(azureStorageLogProviderOptions, logEntry, jsonSettings);
            var expectedTemplate =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";
            var expectedMessage =
                $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} ResponseId: {responseId}";
            var expectedLogEntry = new LogEntry
                                   {
                                       Exception = new Exception(expectedMessage),
                                       Level = "Warning",
                                       MessageTemplate = expectedTemplate,
                                       Timestamp = dt,
                                       EventId = new EventId(7437)
                                   };
            var expectedJsonString = JsonConvert.SerializeObject(expectedLogEntry, jsonSettings);

            Assert.AreEqual(expectedJsonString, result);
        }

        /// <summary>
        ///     Test for the extension method that creates a log message
        /// </summary>
        [Test]
        public void TestToLogMessage()
        {
            var requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);

            var azureStorageLogProviderOptions = new AzureStorageLogProviderOptions("https://test.com", true);
            var template =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";
            var result = azureStorageEventLogResult.ToLogMessage(azureStorageLogProviderOptions, template);
            var expected =
                $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} ResponseId: {responseId}";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the extension method that returns the right message template (Failure)
        /// </summary>
        [Test]
        public void TestToMessageTemplateFailed()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com", true);

            var requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = false;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);

            var result = azureStorageEventLogResult.ToMessageTemplate(options);
            var expected =
                "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the extension method that returns the right message template (Success)
        /// </summary>
        [Test]
        public void TestToMessageTemplateSuccess()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com", true);

            var requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);

            var result = azureStorageEventLogResult.ToMessageTemplate(options);
            var expected =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";

            Assert.AreEqual(expected, result);
        }
    }
}