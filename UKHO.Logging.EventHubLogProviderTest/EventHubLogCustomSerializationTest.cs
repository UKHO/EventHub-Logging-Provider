// British Crown Copyright © 2023,
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
using System.Text;

using FakeItEasy;

using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    public class EventHubLogCustomSerializationTest
    {
        [Test]
        public void TestSerializationCustomisation()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient, new JsonConverter[] { new VersionJsonConverter() });
            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   LogProperties = new Dictionary<string, object>
                                                   { { "version", new VersionObjectForTest { MajorVersion = 1, MinorVersion = 2, Build = 3, Revision = 456 } } },
                                   MessageTemplate = "Hello this is a message template for version {version}",
                                   Level = "Debug"
                               };
            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);

            StringAssert.Contains("\"version\": \"1.2.3.456\"", sentString);
            StringAssert.DoesNotContain("Major", sentString);
            StringAssert.DoesNotContain("Minor", sentString);
            StringAssert.DoesNotContain("Build", sentString);
            StringAssert.DoesNotContain("Revision", sentString);
        }

        [Test]
        public void TestErrorDuringSerilizationIsLogged()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient, new JsonConverter[] { new BadVersionJsonConveter() });
            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   LogProperties = new Dictionary<string, object> { { "version", new VersionObjectForTest("1.2.3.4") } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "LogLevel"
                               };

            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreNotEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual("Log Serialization failed with exception", sentLogEntry.MessageTemplate);

            Assert.AreNotEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(7437, sentLogEntry.EventId.Id);
            Assert.AreEqual("LogSerializationException", sentLogEntry.EventId.Name);

            Assert.AreNotEqual(testLogEntry.Exception, sentLogEntry.Exception);
            Assert.AreEqual(GetType().Assembly.GetName().Name, sentLogEntry.Exception.Source);
            Assert.AreEqual("This converter exploded for test reasons", sentLogEntry.Exception.Message);

            Assert.AreNotEqual(testLogEntry.Level, sentLogEntry.Level);
            Assert.AreEqual("Warning", sentLogEntry.Level);
        }

        [Test]
        public void TestReallyBadConvertedDoesNotPreventLoggingOfError()
        {
            var fakeEventHubClient = A.Fake<IEventHubClientWrapper>();

            byte[] sentBytes = null;
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).Invokes((EventData ed) => sentBytes = ed.Body.Array);

            var eventHubLog = new EventHubLog(fakeEventHubClient, new JsonConverter[] { new ReallyBadJsonConveter() });
            var testLogEntry = new LogEntry
                               {
                                   EventId = new EventId(2),
                                   Timestamp = new DateTime(2002, 03, 04),
                                   LogProperties = new Dictionary<string, object> { { "version", new VersionObjectForTest("1.2.3.4") } },
                                   MessageTemplate = "Hello this is a message template",
                                   Level = "LogLevel"
                               };

            eventHubLog.Log(testLogEntry);
            A.CallTo(() => fakeEventHubClient.SendAsync(A<EventData>.Ignored)).MustHaveHappenedOnceExactly();

            var sentString = Encoding.UTF8.GetString(sentBytes);
            var sentLogEntry = JsonConvert.DeserializeObject<LogEntry>(sentString);
            Assert.AreNotEqual(testLogEntry.LogProperties, sentLogEntry.LogProperties);
            Assert.AreEqual("Log Serialization failed with exception", sentLogEntry.MessageTemplate);
            
            Assert.AreNotEqual(testLogEntry.EventId, sentLogEntry.EventId);
            Assert.AreEqual(7437, sentLogEntry.EventId.Id);
            Assert.AreEqual("LogSerializationException", sentLogEntry.EventId.Name);

            Assert.AreNotEqual(testLogEntry.Exception, sentLogEntry.Exception);
            Assert.AreEqual(GetType().Assembly.GetName().Name, sentLogEntry.Exception.Source);
            Assert.AreEqual("This is a really bad JsonConverter that says it can convert anything, and then throws", sentLogEntry.Exception.Message);

            Assert.AreNotEqual(testLogEntry.Level, sentLogEntry.Level);
            Assert.AreEqual("Warning", sentLogEntry.Level);
        }
    }

    internal class VersionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is VersionObjectForTest version)
            {
                writer.WriteValue($"{version.MajorVersion}.{version.MinorVersion}.{version.Build}.{version.Revision}");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VersionObjectForTest);
        }

        public override bool CanRead => false;
    }

    internal class BadVersionJsonConveter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("This converter exploded for test reasons");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VersionObjectForTest);
        }
    }

    internal class ReallyBadJsonConveter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("This is a really bad JsonConverter that says it can convert anything, and then throws");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}