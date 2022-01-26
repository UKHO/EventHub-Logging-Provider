using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;


namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class AzureStorageEventLoggerExtensionsTests
    {
        [Test]
        public void Test_IsLongMessage_WithMessageGreaterThanMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 1025);
            bool result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_IsLongMessage_WithMessageLessThanMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 512);
            bool result = message.IsLongMessage(size);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_IsLongMessage_WithMessageEqualToMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 1024);
            bool result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = null;
            string message = GenerateTestMessage(1024 * 1024);
            bool result =  azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message,1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNotNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(null);
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNotNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledTrue()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledFalse()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", false));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageGreaterThanDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1025);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageEqualToDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageLessThanDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 512);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void TestGetLogEntryPropertyValueKeyExists()
        {
            Dictionary<string, object> set = new Dictionary<string, object>();
            set.Add("test_key_exists","test_value");
            string result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNotNull(result);
            Assert.AreEqual("test_value", result);
        }
        [Test]
        public void TestGetLogEntryPropertyValueKeyNotExists()
        {
            Dictionary<string, object> set = new Dictionary<string, object>();
            set.Add("test_key__not_exists", "test_value");
            string result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNull(result);
        }

        //ToJsonLogEntryString
        [Test]
        public void TestToLogMessage()
        {
            string requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            string reasonPhrase = "Tested";
            int statusCode = 201;
            bool isStored = true;
            string blobFullName = String.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            AzureStorageEventLogResult azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase,statusCode,requestId, responseId, isStored,blobFullName);


            AzureStorageLogProviderOptions azureStorageLogProviderOptions = new AzureStorageLogProviderOptions("https://test.com",true);
            string template = "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";
            string result = azureStorageEventLogResult.ToLogMessage(azureStorageLogProviderOptions, template);
            string expected = $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} ResponseId: {responseId}";
            Assert.AreEqual(expected,result);
        }

        [Test]
        public void TestToJsonLogEntryString()
        {
            DateTime dt = DateTime.UtcNow;
            string requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            string reasonPhrase = "Tested";
            int statusCode = 201;
            bool isStored = true;
            string blobFullName = String.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            AzureStorageEventLogResult azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);
            AzureStorageLogProviderOptions azureStorageLogProviderOptions = new AzureStorageLogProviderOptions("https://test.com", true);
            var logEntry = new LogEntry()
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
            string result = azureStorageEventLogResult.ToJsonLogEntryString(azureStorageLogProviderOptions,logEntry, jsonSettings);
            string expectedTemplate = "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";
            string expectedMessage = $"Azure Storage Logging: A blob with the error details was created at {blobFullName}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {reasonPhrase} ResponseCode: {statusCode} RequestId: {requestId} ResponseId: {responseId}";
            var expectedLogEntry = new LogEntry()
                                   {
                                       Exception = new Exception(expectedMessage),
                                       Level = "Warning",
                                       MessageTemplate = expectedTemplate,
                                       Timestamp = dt,
                                       EventId = new EventId(7437)
                                   };
            var expectedJsonString = JsonConvert.SerializeObject(expectedLogEntry, jsonSettings);

            Assert.AreEqual(expectedJsonString,result);
        }
        [Test]
        public void TestToMessageTemplateSuccess()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com",true);

            string requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            string reasonPhrase = "Tested";
            int statusCode = 201;
            bool isStored = true;
            string blobFullName = String.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            AzureStorageEventLogResult azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);

            string result = azureStorageEventLogResult.ToMessageTemplate(options);
            string expected = "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";

            Assert.AreEqual(expected,result);
        }

        [Test]
        public void TestToMessageTemplateFailed()
        {
            var options = new AzureStorageLogProviderOptions("https://test.com", true);

            string requestId = Guid.NewGuid().ToString();
            long responseId = 123456;
            string reasonPhrase = "Tested";
            int statusCode = 201;
            bool isStored = false;
            string blobFullName = String.Format("{0}.{1}", Guid.NewGuid().ToString().Replace("-", "_"), "blob");
            AzureStorageEventLogResult azureStorageEventLogResult = new AzureStorageEventLogResult(reasonPhrase, statusCode, requestId, responseId, isStored, blobFullName);

            string result = azureStorageEventLogResult.ToMessageTemplate(options);
            string expected = "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} ResponseId: {{ResponseId}}";

            Assert.AreEqual(expected, result);
        }






        /// <summary>
        /// Generates a test string message
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns>A message</returns>
        private string GenerateTestMessage(int size)
        {
            var charsPool = "ABCDEFGHJKLMNOPQRSTVUWXYZ1234567890";
            var charsArray = new char[size];

            var rand = new Random();

            for (int c = 0; c < charsArray.Length; c++)
            {
                charsArray[c] = charsPool[rand.Next(charsPool.Length)];
            }

            return new String(charsArray);
        }
    }
}
