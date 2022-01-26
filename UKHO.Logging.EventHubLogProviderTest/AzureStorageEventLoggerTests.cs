using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure.Storage.Blobs;
using FakeItEasy;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.Logging.AzureStorageEventLogging;
using UKHO.Logging.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider.Settings;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     The Azure Storage Event Logger Test Class
    /// </summary>
    [TestFixture]
    internal class AzureStorageEventLoggerTests
    {
        /// <summary>
        ///     Set Up method for the tests
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _blobContainerClient = A.Dummy<BlobContainerClient>();
        }

        private BlobContainerClient _blobContainerClient;

        public void TestAZS()
        {
            var options = new AzureStorageLogProviderOptions(AppSettings.GetSetting("Logs.Queue.Container.SasUrl"), true);
            var blobClient = new BlobContainerClient(new Uri(AppSettings.GetSetting("Logs.Queue.Container.SasUrl")));
            var azureModel = new AzureStorageEventLogger(blobClient);
            var blobFullName = azureModel.GenerateBlobFullName(
                                                               new AzureStorageBlobFullNameModel(azureModel.GenerateServiceName(
                                                                                                  "ees",
                                                                                                  "dev"),
                                                                                                 azureModel.GeneratePathForErrorBlob(DateTime.Now),
                                                                                                 azureModel.GenerateErrorBlobName()));
            var azureStorageModel = new AzureStorageEventModel(blobFullName, GenerateTestMessage(1024 * 1024));

            var azureStorageResult = azureModel.StoreLogFile(azureStorageModel);
            var template = azureStorageResult.IsStored ? options.SuccessfulMessageTemplate : options.FailedMessageTemplate;
            var resultMessage = azureStorageResult.ToLogMessage(options, template);
        }

        /// <summary>
        ///     Test for the cancellation operation
        /// </summary>
        public void TestEventHubLogFMessageCancellation()
        {
            var blobClient = new BlobContainerClient(new Uri(AppSettings.GetSetting("Logs.Queue.Container.SasUrl")));
            var azureModel = new AzureStorageEventLogger(blobClient);
            var blobFullName = azureModel.GenerateBlobFullName(
                                                               new AzureStorageBlobFullNameModel(azureModel.GenerateServiceName(
                                                                                                  "ees",
                                                                                                  "dev"),
                                                                                                 azureModel.GeneratePathForErrorBlob(DateTime.Now),
                                                                                                 azureModel.GenerateErrorBlobName()));
            var azureStorageModel = new AzureStorageEventModel(blobFullName, GenerateTestMessage(1024 * 1024 * 15));

            var azureStorageResult = azureModel.StoreLogFile(azureStorageModel, true);
            var res = azureModel.CancelLogFileStoringOperation();
        }

        #region string builders

        /// <summary>
        ///     Test for the method that generates the service name
        /// </summary>
        [Test]
        public void TestGenerateServiceName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(_blobContainerClient);
            var serviceName = "testService";
            var environment = "testEnvironment";
            var result = azureStorageLogger.GenerateServiceName(serviceName, environment);
            var expected = string.Format("{0} - {1}", serviceName, environment);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the method that generates a path for the error blob
        /// </summary>
        [Test]
        public void TestGeneratePathForErrorBlob()
        {
            var azureStorageLogger = new AzureStorageEventLogger(_blobContainerClient);
            var dt = new DateTime(2021, 12, 10, 12, 13, 14);
            //DateTime.Parse("2020-12-10 12:13:14");
            var result = azureStorageLogger.GeneratePathForErrorBlob(dt);
            var expected = "2021\\12\\10\\12\\13\\14";
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the method that generates the blob name (With Guid Only)
        /// </summary>
        [Test]
        public void TestGenerateErrorBlobName_WithGuidOnly()
        {
            var azureStorageLogger = new AzureStorageEventLogger(_blobContainerClient);
            var name = Guid.NewGuid();
            var result = azureStorageLogger.GenerateErrorBlobName(name);
            var expected = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), "txt");
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the method that generates the blob name (With Guid and Extension)
        /// </summary>
        [Test]
        public void TestGenerateErrorBlobName_WithGuidAndExtension()
        {
            var azureStorageLogger = new AzureStorageEventLogger(_blobContainerClient);
            var name = Guid.NewGuid();
            var extension = "json";
            var result = azureStorageLogger.GenerateErrorBlobName(name, extension);
            var expected = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), extension);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the method that generates the blob full name
        /// </summary>
        [Test]
        public void TestGenerateBlobFullName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(_blobContainerClient);
            var name = Guid.NewGuid();
            var extension = "txt";
            var serviceName = "testService";
            var path = "2021\\12\\10\\12\\13\\14";
            var blobName = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), extension);
            var azureSotrageBlobModel = new AzureStorageBlobFullNameModel(serviceName, path, blobName);
            var result = azureStorageLogger.GenerateBlobFullName(azureSotrageBlobModel);
            var expected = Path.Combine(serviceName, path, blobName);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region StoreMessage

        /// <summary>
        ///     Test for the method that stores the message (When the message is greater than 1 MB)
        /// </summary>
        [Test]
        public void TestEventHubLogForMessagesGreaterTo1MB()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();
            var service = "test service";
            var environment = "test environment";
            var testLogProperties = new Dictionary<string, object> { { "_Service", service }, { "_Environment", environment } };
            var testDateStamp = new DateTime(2002, 03, 04);
            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);
            var template = GenerateTestMessage(1024 * 1024);
            var eventHubLog = new EventHubLog(fakeEventHubClient);

            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = testDateStamp,
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = testLogProperties,
                                   MessageTemplate = template, //find the size of the rest of the object so that we can create it exactly 1 mb
                                   Level = "LogLevel"
                               };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);

            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            CollectionAssert.AreEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
            Assert.AreEqual(2, sentLogEntry.LogProperties.Count);
            Assert.AreEqual(testLogEntry.LogProperties.First(), sentLogEntry.LogProperties.First());

            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            var azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            var blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                                                  azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                                                  azureLogger.GenerateErrorBlobName()));

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("Azure Storage Logging:"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("Azure Storage Logging:"));
        }

        /// <summary>
        ///     Test for the method that stores the message (When the message is equal to 1 MB)
        /// </summary>
        [Test]
        public void TestEventHubLogForMessagesEqualTo1MB()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();
            var service = "test service";
            var environment = "test environment";
            var testLogProperties = new Dictionary<string, object> { { "_Service", service }, { "_Environment", environment } };
            var testDateStamp = new DateTime(2002, 03, 04);
            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);
            string template = null;
            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = testDateStamp,
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = testLogProperties,
                                   MessageTemplate = string.Empty, //find the size of the rest of the object so that we can create it exactly 1 mb
                                   Level = "LogLevel"
                               };
            template = CreateMessageEqualTo1MB(testLogEntry);
            testLogEntry.MessageTemplate = template;
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);

            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            CollectionAssert.AreEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
            Assert.AreEqual(2, sentLogEntry.LogProperties.Count);
            Assert.AreEqual(testLogEntry.LogProperties.First(), sentLogEntry.LogProperties.First());

            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            var azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            var blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                                                  azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                                                  azureLogger.GenerateErrorBlobName()));

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("Azure Storage Logging:"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("Azure Storage Logging:"));
        }

        /// <summary>
        ///     Test for the method that stores the message (When the message is less than 1 MB)
        /// </summary>
        [Test]
        public void TestEventHubLogForMessagesLessThan1MB()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();
            var service = "test service";
            var environment = "test environment";
            var testLogProperties = new Dictionary<string, object> { { "_Service", service }, { "_Environment", environment } };
            var testDateStamp = new DateTime(2002, 03, 04);
            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);
            var template = GenerateTestMessage(1024 * 512);
            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = testDateStamp,
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = testLogProperties,
                                   MessageTemplate = template,
                                   Level = "LogLevel"
                               };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);

            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            CollectionAssert.AreEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
            Assert.AreEqual(2, sentLogEntry.LogProperties.Count);
            Assert.AreEqual(testLogEntry.LogProperties.First(), sentLogEntry.LogProperties.First());

            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            var azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            var blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                                                  azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                                                  azureLogger.GenerateErrorBlobName()));

            Assert.AreEqual(sentLogEntry.MessageTemplate, template);
            Assert.AreEqual(testLogEntry.Exception.Message, sentLogEntry.Exception.Message);
        }

        #endregion

        #region help functions

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
        ///     Returns the size of the entry model when using the EventHub Log serialization settings
        /// </summary>
        /// <param name="entry">The entry</param>
        /// <returns>The size of the message</returns>
        private int GetSizeOfJsonObject(LogEntry entry)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry,
                                                                      new JsonSerializerSettings
                                                                      {
                                                                          Formatting = Formatting.Indented,
                                                                          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                                          ContractResolver = new NullPropertyResolver()
                                                                      })).Length;
        }

        /// <summary>
        ///     Calculates the message template and creates so that the total size of the json as 1 MB
        /// </summary>
        /// <param name="entry">The entry model</param>
        /// <returns>The message template</returns>
        private string CreateMessageEqualTo1MB(LogEntry entry)
        {
            return GenerateTestMessage(1024 * 1024 - GetSizeOfJsonObject(entry));
        }

        #endregion
    }
}