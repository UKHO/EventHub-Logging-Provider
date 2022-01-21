﻿// British Crown Copyright © 2018,
// All rights reserved.
// 
// You may not copy the Software, rent, lease, sub-license, loan, translate, merge, adapt, vary
// re-compile or modify the Software without written permission from UKHO.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL CROWN OR THE SECRETARY OF STATE FOR DEFENCE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
// BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
// IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
// OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FakeItEasy;

using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    class EventHubLogTests
    {
        [Test]
        public void TestHandelsHandelsCircularReferencesCorrectly()
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
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "Debug"
                               };
            testLogEntry.LogProperties.Add("circular", testLogEntry);

            eventHubLog.Log(testLogEntry);

            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(testLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            CollectionAssert.AreEqual(testLogEntry.LogProperties.Take(1), sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
        }

        [Test]
        public void TestSerializesJsonCorrctlyForCorrectLogEntry()
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
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "Debug"
                               };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(testLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            CollectionAssert.AreEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
        }

        [Test]
        public void TestSerializesJsonWhenPropertyThrowsException()
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
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" }, { "thowable", new ObjectThatThrows() } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "LogLevel"
                               };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(testLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(testLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);

            Assert.AreEqual(2, sentLogEntry.LogProperties.Count);
            Assert.AreEqual(testLogEntry.LogProperties.First(),sentLogEntry.LogProperties.First());

            var asJObject = (JObject)sentLogEntry.LogProperties.Skip(1).First().Value;

            Assert.AreEqual(new ObjectThatThrows().NotThrowable, asJObject["NotThrowable"].Value<string>());

            Assert.AreEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(testLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(testLogEntry.Level, sentLogEntry.Level);
         }

        [Test]
        public void TestSerializesJsonThrowsExceptionThisExceptionIsLogged()
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
                LogProperties = new Dictionary<string, object> { { "hi", "Guys" }, { "thowable", new ObjectThatThrowsAfterOneGet() } },
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
                                   LogProperties = new Dictionary<string, object> { { "_Service", "ees" }, { "_Environment",  "dev" } },
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

            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
            {
                EventId = new EventId(2),
                Timestamp = testDateStamp,
                Exception = new InvalidOperationException("TestLoggedException"),
                LogProperties = testLogProperties,
                MessageTemplate = this.GenerateTestMessage(1024 * 1024),  //find the size of the rest of the object so that we can create it exactly 1 mb
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

            AzureStorageEventLogger azureLogger = new AzureStorageEventLogger();

            string blobFullName = azureLogger.GenerateBlobFullName(azureLogger.GenerateServiceName(service, environment),
                                                                   azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                   azureLogger.GenerateErrorBlobName());

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("A blob created at"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("A blob created at"));
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

            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var testLogEntry = new LogEntry()
            {
                EventId = new EventId(2),
                Timestamp = testDateStamp,
                Exception = new InvalidOperationException("TestLoggedException"),
                LogProperties = testLogProperties,
                MessageTemplate = this.GenerateTestMessage(1024 * 1024),
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

            AzureStorageEventLogger azureLogger = new AzureStorageEventLogger();

            string blobFullName = azureLogger.GenerateBlobFullName(azureLogger.GenerateServiceName(service, environment),
                                                                   azureLogger.GeneratePathForErrorBlob(testDateStamp),
                                                                   azureLogger.GenerateErrorBlobName());

            Assert.IsTrue(sentLogEntry.MessageTemplate.StartsWith("A blob created at"));
            Assert.IsTrue(sentLogEntry.Exception.Message.StartsWith("A blob created at"));
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

            for (int c =0; c < charsArray.Length; c++)
            {
                charsArray[c] = charsPool[rand.Next(charsPool.Length)];
            }

            return new String(charsArray);
        }
    }

    

    internal class ObjectThatThrows
    {
        public string Throwable
        {
            get { throw new Exception("Thrown on throwable getter"); }
            set { }
        }

        public string NotThrowable { get; set; } ="NotThrowing";
    }

    internal class ObjectThatThrowsAfterOneGet
    {
        private bool hasBeenGot;
        public string Throwable
        {
            get
            {
                if (hasBeenGot)
                    throw new Exception("Thrown on throwable getter");
                hasBeenGot = true;
                return "Gotten";
            }
        }
    }
}