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
using UKHO.Logging.EventHubLogProvider;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

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
            blobContainerClient = A.Dummy<BlobContainerClient>();
        }

        private BlobContainerClient blobContainerClient;

        #region string builders

        /// <summary>
        ///     Test for the method that generates the service name
        /// </summary>
        [Test]
        public void Test_GenerateServiceName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
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
        public void Test_GeneratePathForErrorBlob()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
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
        public void Test_GenerateErrorBlobName_WithGuidOnly()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
            var name = Guid.NewGuid();
            var result = azureStorageLogger.GenerateErrorBlobName(name);
            var expected = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), "json");
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Test for the method that generates the blob name (With Guid and Extension)
        /// </summary>
        [Test]
        public void Test_GenerateErrorBlobName_WithGuidAndExtension()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
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
        public void Test_GenerateBlobFullName()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
            var name = Guid.NewGuid();
            var extension = "json";
            var serviceName = "testService";
            var path = "2021\\12\\10\\12\\13\\14";
            var blobName = string.Format("{0}.{1}", name.ToString().Replace("-", "_"), extension);
            var azureStorageBlobModel = new AzureStorageBlobFullNameModel(serviceName, path, blobName);
            var result = azureStorageLogger.GenerateBlobFullName(azureStorageBlobModel);
            var expected = Path.Combine(serviceName, path, blobName);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region StoreMessage

        /// <summary>
        ///     Test for the cancellation operation (When unable to cancel)
        /// </summary>
        [Test]
        public void Test_EventHubLogFMessageCancellation_UnableToCancel()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);

            var result = azureStorageLogger.CancelLogFileStoringOperation();

            Assert.AreEqual(AzureStorageEventLogCancellationResult.UnableToCancel, result);
        }

        /// <summary>
        ///     Test for the cancellation operation (When able to cancel)
        /// </summary>
        [Test]
        public void Test_EventHubLogFMessageCancellation_CancelSuccessfully()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
            var azureStorageModel = new AzureStorageEventModel("test service - test environment/day/test.json", GenerateTestMessage(512 * 512));
            azureStorageLogger.StoreLogFile(azureStorageModel, true);
            var result = azureStorageLogger.CancelLogFileStoringOperation();

            Assert.AreEqual(AzureStorageEventLogCancellationResult.Successful, result);
        }

        /// <summary>
        ///     Test for the cancellation operation (When cancellation fails)
        /// </summary>
        [Test]
        public void Test_EventHubLogFMessageCancellation_CancelFailed()
        {
            var azureStorageLogger = new AzureStorageEventLogger(blobContainerClient);
            var azureStorageModel = new AzureStorageEventModel("test service - test environment/day/test.json", GenerateTestMessage(512 * 512));
            azureStorageLogger.StoreLogFile(azureStorageModel, true);
            azureStorageLogger.NullifyTokenSource();
            var result = azureStorageLogger.CancelLogFileStoringOperation();

            Assert.AreEqual(AzureStorageEventLogCancellationResult.CancellationFailed, result);
        }

        /// <summary>
        ///     Test for the method that stores the message (When the message is greater than 1 MB)
        /// </summary>
        [Test]
        public void Test_EventHubLog_ForMessagesGreaterTo1MB()
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
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            var azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("Azure Storage Logging:"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("Azure Storage Logging:"));
        }

        /// <summary>
        ///     Test for the method that stores the message (When the message is equal to 1 MB)
        /// </summary>
        [Test]
        public void Test_EventHubLog_ForMessagesEqualTo1MB()
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
            template = CreateMessageEqualTo1Mb(testLogEntry);
            testLogEntry.MessageTemplate = template;
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);

            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            CollectionAssert.AreEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);

            var azureLogger = new AzureStorageEventLogger(fakeEventHubClient.AzureStorageBlobContainerBuilder.BlobContainerClient);

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("Azure Storage Logging:"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("Azure Storage Logging:"));
        }

        /// <summary>
        ///     Test for the method that stores the message (When the message is less than 1 MB)
        /// </summary>
        [Test]
        public void Test_EventHubLog_ForMessagesLessThan1MB()
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
        private string CreateMessageEqualTo1Mb(LogEntry entry)
        {
            return GenerateTestMessage(1024 * 1024 - GetSizeOfJsonObject(entry));
        }

        #endregion
    }
}