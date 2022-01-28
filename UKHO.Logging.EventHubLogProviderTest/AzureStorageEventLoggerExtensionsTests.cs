using System;
using System.Collections.Generic;
using Azure;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProviderTest.Factories;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     Tests for the Azure storage event logger extensions
    /// </summary>
    [TestFixture]
    public class AzureStorageEventLoggerExtensionsTests
    {
        private readonly ResourcesFactory resourcesFactory = new ResourcesFactory();

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
        ///     Tests the get file size extension method (when response is null)
        /// </summary>
        [Test]
        public void Test_GetFileSize_ResponseNull()
        {
            var response = (Response)null;
            var fileSize = 1024;
            var result = response.GetFileSize(fileSize);
            var expected = 1024;
            Assert.AreEqual(result, expected);
        }

        /// <summary>
        ///     tests the get file size extension method (when response stream is null)
        /// </summary>
        [Test]
        public void Test_GetFileSize_ResponseStreamNull()
        {
            var response = A.Fake<Response>();
            var fileSize = 1024;
            var result = response.GetFileSize(fileSize);
            var expected = 1024;
            Assert.AreEqual(result, expected);
        }

        /// <summary>
        ///     Tests for the LogEntry Get Property Value (When key exists)
        /// </summary>
        [Test]
        public void Test_GetLogEntryPropertyValue_KeyExists()
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
        public void Test_GetLogEntryPropertyValue_KeyNotExists()
        {
            var set = new Dictionary<string, object>();
            set.Add("test_key__not_exists", "test_value");
            var result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNull(result);
        }

        /// <summary>
        ///     Tests the get modified date extension method (when header is null)
        /// </summary>
        [Test]
        public void Test_GetModifiedDate_ResponseHeaderNull()
        {
            var response = (Response)null;
            var result = response.GetModifiedDate();
            DateTime? expected = null;
            Assert.AreEqual(result, expected);
        }

        /// <summary>
        ///     Tests the get modified date extension method (when stream is null)
        /// </summary>
        [Test]
        public void Test_GetModifiedDate_ResponseStreamNull()
        {
            var response = A.Fake<Response>();
            var result = response.GetModifiedDate();
            DateTime? expected = null;
            Assert.AreEqual(result, expected);
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        false,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
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
            var azureStorageBlobContainerBuilder =
                new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com",
                                                                                        true,
                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                        resourcesFactory.FailureTemplateMessage));
            var message = GenerateTestMessage(1024 * 512);
            var result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Test for the extension method that creates and returns the log entry as a json string
        /// </summary>
        [Test]
        public void Test_ToJsonLogEntryString()
        {
            var requestId = Guid.NewGuid().ToString();
            var sha = Guid.NewGuid().ToString();
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var modifiedDate = DateTime.UtcNow;
            long fileSize = 12345678;
            var dt = DateTime.UtcNow;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, sha, isStored, blobFullName, fileSize, modifiedDate);
            var azureStorageLogProviderOptions =
                new AzureStorageLogProviderOptions("https://test.com", true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);
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
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}";
            var expectedMessage =
                $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} Sha256: {sha} FileSize(Bs): {fileSize} FileModifiedDate: {modifiedDate}";
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
        public void Test_ToLogMessage()
        {
            var requestId = Guid.NewGuid().ToString();
            var sha = Guid.NewGuid().ToString();
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var modifiedDate = DateTime.UtcNow;
            long fileSize = 12345678;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, sha, isStored, blobFullName, fileSize, modifiedDate);

            var azureStorageLogProviderOptions =
                new AzureStorageLogProviderOptions("https://test.com", true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);
            var template =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}";
            var result = azureStorageEventLogResult.ToLogMessage(azureStorageLogProviderOptions, template);
            var expected =
                $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} Sha256: {sha} FileSize(Bs): {fileSize} FileModifiedDate: {modifiedDate}";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the extension method that returns the right message template (Failure)
        /// </summary>
        [Test]
        public void Test_ToMessageTemplate_Failed()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com", true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);

            var requestId = Guid.NewGuid().ToString();
            var sha = Guid.NewGuid().ToString();
            var reasonPhrase = "Tested";
            var statusCode = 403;
            var isStored = false;
            var modifiedDate = DateTime.UtcNow;
            long fileSize = 12345678;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, sha, isStored, blobFullName, fileSize, modifiedDate);

            var result = azureStorageEventLogResult.ToMessageTemplate(options);
            var expected =
                "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the extension method that returns the right message template (Success)
        /// </summary>
        [Test]
        public void Test_ToMessageTemplate_Success()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com", true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);

            var requestId = Guid.NewGuid().ToString();
            var sha = Guid.NewGuid().ToString();
            var reasonPhrase = "Tested";
            var statusCode = 201;
            var isStored = true;
            var modifiedDate = DateTime.UtcNow;
            long fileSize = 12345678;
            var blobFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            var azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, sha, isStored, blobFullName, fileSize, modifiedDate);

            var result = azureStorageEventLogResult.ToMessageTemplate(options);
            var expected =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}";

            Assert.AreEqual(expected, result);
        }
    }
}