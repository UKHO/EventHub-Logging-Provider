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
        private BlobContainerClient _blobContainerClient;
        [SetUp]
        public void SetUp()
        {
            this._blobContainerClient = A.Dummy<BlobContainerClient>();
        }

        [Test]
        public void TestAZS()
        {
            var options = new AzureStorageLogProviderOptions(AppSettings.GetSetting("Logs.Queue.Container.SasUrl"),true);
            var blobClient = new BlobContainerClient(new Uri(AppSettings.GetSetting("Logs.Queue.Container.SasUrl")));
            var azureModel = new AzureStorageEventLogger(blobClient);
            string blobFullName = azureModel.GenerateBlobFullName(
                                                                  new AzureStorageBlobFullNameModel(azureModel.GenerateServiceName(
                                                                                                     "ees", "dev"),
                                                                                                    azureModel.GeneratePathForErrorBlob(DateTime.Now),
                                                                                                    azureModel.GenerateErrorBlobName()));
            var azureStorageModel = new AzureStorageEventModel(blobFullName, this.GenerateTestMessage(1024 * 1024));

            var azureStorageResult = azureModel.StoreLogFile(azureStorageModel);
            string template = (azureStorageResult.IsStored ? options.SuccessfulMessageTemplate :
                options.FailedMessageTemplate);
            string resultMessage = azureStorageResult.ToLogMessage(options,template);
        }
        public void TestEventHubLogFMessageCancellation()
        {
           
            var blobClient = new BlobContainerClient(new Uri(AppSettings.GetSetting("Logs.Queue.Container.SasUrl")));
            var azureModel = new AzureStorageEventLogger(blobClient);
            string blobFullName = azureModel.GenerateBlobFullName(
                                                                  new AzureStorageBlobFullNameModel(azureModel.GenerateServiceName(
                                                                                                     "ees", "dev"),
                                                                                                    azureModel.GeneratePathForErrorBlob(DateTime.Now),
                                                                                                    azureModel.GenerateErrorBlobName()));
            var azureStorageModel = new AzureStorageEventModel(blobFullName, this.GenerateTestMessage((1024 * 1024 * 15)));
             
            var azureStorageResult = azureModel.StoreLogFile(azureStorageModel,true);
            var res = azureModel.CancelLogFileStoringOperation();
        }
        #region string builders
        [Test]
        public void TestGenerateServiceName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(this._blobContainerClient);
            string serviceName = "testService";
            String environment = "testEnvironment";
            string result = azureStorageLogger.GenerateServiceName(serviceName,environment);
            string expected = String.Format("{0} - {1}", serviceName, environment);
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void TestGeneratePathForErrorBlob()
        {
            var azureStorageLogger = new AzureStorageEventLogger(this._blobContainerClient);
            DateTime dt = new DateTime(2021,12,10,12,13,14);
                //DateTime.Parse("2020-12-10 12:13:14");
            string result = azureStorageLogger.GeneratePathForErrorBlob(dt);
            string expected = "2021\\12\\10\\12\\13\\14";
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void TestGenerateErrorBlobName_WithGuidOnly()
        {
            var azureStorageLogger = new AzureStorageEventLogger(this._blobContainerClient);
            Guid name = Guid.NewGuid();
            string result = azureStorageLogger.GenerateErrorBlobName(name);
            string expected = string.Format("{0}.{1}",name.ToString().Replace("-", "_"), "txt");
            Assert.AreEqual(expected,result);

        }
        [Test]
        public void TestGenerateErrorBlobName_WithGuidAndExtension()
        {
            var azureStorageLogger = new AzureStorageEventLogger(this._blobContainerClient);
            Guid name = Guid.NewGuid();
            string extension = "json";
            string result = azureStorageLogger.GenerateErrorBlobName(name,extension);
            string expected = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), extension);
            Assert.AreEqual(expected, result);

        }
        [Test]
        public void TestGenerateBlobFullName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(this._blobContainerClient);
            Guid name = Guid.NewGuid();
            string extension = "txt";
            string serviceName = "testService";
            string path = "2021\\12\\10\\12\\13\\14";
            string blobName = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), extension);
            var azureSotrageBlobModel = new AzureStorageBlobFullNameModel(serviceName,path,blobName);
            string result = azureStorageLogger.GenerateBlobFullName(azureSotrageBlobModel);
            string expected = Path.Combine(serviceName,path,blobName);
            Assert.AreEqual(expected,result);
        }


        #endregion

        #region StoreMessage
        [Test]
        public void TestEventHubLogForMessagesGreaterTo1MB()
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
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("A blob was created"));
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
            string template = null;
            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
            {
                EventId = new EventId(2),
                Timestamp = testDateStamp,
                Exception = new InvalidOperationException("TestLoggedException"),
                LogProperties = testLogProperties,
                MessageTemplate = string.Empty,  //find the size of the rest of the object so that we can create it exactly 1 mb
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

            AzureStorageEventLogger azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            string blobFullName = azureLogger.GenerateBlobFullName(new AzureStorageBlobFullNameModel(azureLogger.GenerateServiceName(service, environment),
                                                                   azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                   azureLogger.GenerateErrorBlobName()));

            Assert.AreEqual(testLogEntry.MessageTemplate, template);
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("A blob was created"));
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
            var template = this.GenerateTestMessage(1024 * 512);
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
            Assert.AreEqual(testLogEntry.Exception.Message, sentLogEntry.Exception.Message);
        }
        #endregion
        #region help functions
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

        /// <summary>
        /// Returns the size of the entry model when using the EventHub Log serialization settings
        /// </summary>
        /// <param name="entry">The entry</param>
        /// <returns>The size of the message</returns>
        private int GetSizeOfJsonObject(LogEntry entry)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new NullPropertyResolver()
            })).Length;
        }

        /// <summary>
        /// Calculates the message template and creates so that the total size of the json as 1 MB
        /// </summary>
        /// <param name="entry">The entry model</param>
        /// <returns>The message template</returns>
        private String CreateMessageEqualTo1MB(LogEntry entry)
        {
            return GenerateTestMessage((1024 * 1024) - GetSizeOfJsonObject(entry));

        }
        #endregion
    }
}
