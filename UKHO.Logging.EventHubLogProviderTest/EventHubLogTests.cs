// British Crown Copyright © 2018,
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
using System.Threading;

using FakeItEasy;

using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

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
            var mockLogEntry = new LogEntry()
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "Debug"
                               };
            mockLogEntry.LogProperties.Add("circular",mockLogEntry);

            eventHubLog.Log(mockLogEntry);

            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(mockLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(mockLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            CollectionAssert.AreEqual(mockLogEntry.LogProperties.Take(1), sentLogEntry.LogProperties);
            Assert.AreEqual(mockLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(mockLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(mockLogEntry.Level, sentLogEntry.Level);
        }

        [Test]
        public void TestSerializesJsonCorrctlyForCorrectLogEntry()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var mockLogEntry = new LogEntry()
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "Debug"
                               };
            eventHubLog.Log(mockLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(mockLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(mockLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            CollectionAssert.AreEqual(mockLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(mockLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(mockLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(mockLogEntry.Level, sentLogEntry.Level);
        }

        [Test]
        public void TestSerializesJsonWhenPropertyThrowsException()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient);
            var mockLogEntry = new MockLogEntry()
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   Exception = new InvalidOperationException("TestLoggedException"),
                                   LogProperties = new Dictionary<string, object> { { "hi", "Guys" } },
                                   MessageTemplate = "Hello this is a message template"
                               };
            eventHubLog.Log(mockLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreEqual(mockLogEntry.Timestamp, sentLogEntry.Timestamp);
            Assert.AreEqual(mockLogEntry.MessageTemplate, sentLogEntry.MessageTemplate);
            CollectionAssert.AreEqual(mockLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual(mockLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(mockLogEntry.Exception.Message, sentLogEntry.Exception.Message);
            Assert.AreEqual(null, sentLogEntry.Level);
        }
    }

    public class MockLogEntry : ILogEntry
    {
        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("Level")]
        public string Level
        {
            get { throw new AbandonedMutexException("TestException"); }
            set { }
        }

        [JsonProperty("MessageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, object> LogProperties { get; set; }

        public EventId EventId { get; set; }
        public Exception Exception { get; set; }
    }
}