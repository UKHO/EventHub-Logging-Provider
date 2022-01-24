using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azure.Storage.Blobs;

using FakeItEasy;

using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

using UKHO.Logging.AzureStorageEventLogging;
using UKHO.Logging.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider.Settings;
using UKHO.Logging.EventHubLogProviderTest.Factories;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    /// The Azure Storage Event Logger Test Class
    /// </summary>
    [TestFixture]
    class AzureStorageEventLoggerTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void TestAZS()
        {
            var appSettings = new AppSettings();
            var blobClient = new BlobContainerClient(new Uri(appSettings.AzureStorageContainerSasUrl));
            var azureModel = new AzureStorageEventLogger(blobClient);
            string blobFullName = azureModel.GenerateBlobFullName(
                                                                  new AzureStorageBlobFullNameModel(azureModel.GenerateServiceName(
                                                                                                     "ees", "dev"),
                                                                                                    azureModel.GeneratePathForErrorBlob(DateTime.Now),
                                                                                                    azureModel.GenerateErrorBlobName()));
            var azureStorageModel = new AzureStorageEventModel(blobFullName, this.GenerateTestMessage(1024 * 1024));
            var azureStorageResult = azureModel.StoreLogFile(azureStorageModel);
        }

        [Test]
        public void TestAzureSTorage()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
            {
                EventId = new EventId(2),
                Timestamp = new DateTime(2002, 03, 04),
                Exception = new InvalidOperationException("TestLoggedException"),
                LogProperties = new Dictionary<string, object> { { "_Service", "ees" }, { "_Environment", "dev" } },
                MessageTemplate = "Hello this is a message template",
                Level = "LogLevel"
            };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreNotEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreNotEqual(testLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            Assert.AreNotEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);

            Assert.AreNotEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreNotEqual(testLogEntry.Exception, sentLogEntry.Exception);

            Assert.AreEqual("Newtonsoft.Json", sentLogEntry.Exception.Source);

            Assert.AreNotEqual(testLogEntry.Level, sentLogEntry.Level);
        }

        [Test]
        public void TestEventHubLogForMessagesEqualTo1MB()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();
            string service = "test service";
            string environment = "test environment";
            var testLogProperties = new Dictionary<string, object> { { "_Service", service }, { "_Environment", environment } };
            DateTime testDateStamp = new DateTime(2002, 03, 04);
            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);
            var template = this.GenerateTestMessage(1024 * 1024);
            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
            {
                EventId = new EventId(2),
                Timestamp = testDateStamp,
                Exception = new InvalidOperationException("TestLoggedException"),
                LogProperties = testLogProperties,
                MessageTemplate = template,  //find the size of the rest of the object so that we can create it exactly 1 mb
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

            AzureStorageEventLogger azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            string blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                   azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                   azureLogger.GenerateErrorBlobName()));

            Assert.AreEqual(testLogEntry.MessageTemplate, template);
            Assert.AreEqual(sentLogEntry.Exception.Message, "TestLoggedException");
        }

        [Test]
        public void TestEventHubLogForMessagesLessThan1MB()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();
            string service = "test service";
            string environment = "test environment";
            var testLogProperties = new Dictionary<string, object> { { "_Service", service }, { "_Environment", environment } };
            DateTime testDateStamp = new DateTime(2002, 03, 04);
            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);
            var template = this.GenerateTestMessage(1024 * 1024);
            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
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

            AzureStorageEventLogger azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            string blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                                                     azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                                                     azureLogger.GenerateErrorBlobName()));

            Assert.AreEqual(sentLogEntry.MessageTemplate, template);
            Assert.AreEqual(sentLogEntry.Exception.Message, "TestLoggedException");
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
