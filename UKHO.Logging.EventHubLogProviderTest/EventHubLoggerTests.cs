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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FakeItEasy;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class EventHubLoggerTests
    {
        private const string Service = "Test Service";
        private const string System = "Test System";
        private const string NodeName = "Test Node Name";
        private readonly IEventHubLog fakeEventHubLog;

        public EventHubLoggerTests()
        {
            fakeEventHubLog = A.Fake<IEventHubLog>();
        }

        [TestCase(LogLevel.None, LogLevel.Critical)]
        [TestCase(LogLevel.None, LogLevel.Error)]
        [TestCase(LogLevel.None, LogLevel.Warning)]
        [TestCase(LogLevel.None, LogLevel.Information)]
        [TestCase(LogLevel.None, LogLevel.Debug)]
        [TestCase(LogLevel.None, LogLevel.Trace)]
        [TestCase(LogLevel.Critical, LogLevel.Error)]
        [TestCase(LogLevel.Critical, LogLevel.Warning)]
        [TestCase(LogLevel.Critical, LogLevel.Information)]
        [TestCase(LogLevel.Critical, LogLevel.Debug)]
        [TestCase(LogLevel.Critical, LogLevel.Trace)]
        [TestCase(LogLevel.Error, LogLevel.Warning)]
        [TestCase(LogLevel.Error, LogLevel.Information)]
        [TestCase(LogLevel.Error, LogLevel.Debug)]
        [TestCase(LogLevel.Error, LogLevel.Trace)]
        [TestCase(LogLevel.Warning, LogLevel.Information)]
        [TestCase(LogLevel.Warning, LogLevel.Debug)]
        [TestCase(LogLevel.Warning, LogLevel.Trace)]
        [TestCase(LogLevel.Information, LogLevel.Debug)]
        [TestCase(LogLevel.Information, LogLevel.Trace)]
        [TestCase(LogLevel.Debug, LogLevel.Trace)]
        public void EventHubLoggerDoesNotLogWithANonUkhoCategoryWhenLowerLogLevelPassed(LogLevel configLogLevel, LogLevel attemptedLogLevel)
        {
            var eventHubLogger = CreateTestEventHubLogger(configLogLevel, LogLevel.None, "NonUKHO.SomeNamespace.SomeClass", fakeEventHubLog);

            eventHubLogger.Log<object>(attemptedLogLevel, 0, null, null, null);

            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).MustNotHaveHappened();
        }

        [TestCase(LogLevel.None, LogLevel.Critical)]
        [TestCase(LogLevel.None, LogLevel.Error)]
        [TestCase(LogLevel.None, LogLevel.Warning)]
        [TestCase(LogLevel.None, LogLevel.Information)]
        [TestCase(LogLevel.None, LogLevel.Debug)]
        [TestCase(LogLevel.None, LogLevel.Trace)]
        [TestCase(LogLevel.Critical, LogLevel.Error)]
        [TestCase(LogLevel.Critical, LogLevel.Warning)]
        [TestCase(LogLevel.Critical, LogLevel.Information)]
        [TestCase(LogLevel.Critical, LogLevel.Debug)]
        [TestCase(LogLevel.Critical, LogLevel.Trace)]
        [TestCase(LogLevel.Error, LogLevel.Warning)]
        [TestCase(LogLevel.Error, LogLevel.Information)]
        [TestCase(LogLevel.Error, LogLevel.Debug)]
        [TestCase(LogLevel.Error, LogLevel.Trace)]
        [TestCase(LogLevel.Warning, LogLevel.Information)]
        [TestCase(LogLevel.Warning, LogLevel.Debug)]
        [TestCase(LogLevel.Warning, LogLevel.Trace)]
        [TestCase(LogLevel.Information, LogLevel.Debug)]
        [TestCase(LogLevel.Information, LogLevel.Trace)]
        [TestCase(LogLevel.Debug, LogLevel.Trace)]
        public void EventHubLoggerDoesNotLogWithAUkhoCategoryWhenLowerLogLevelPassed(LogLevel ukhoConfigLogLevel, LogLevel attemptedLogLevel)
        {
            var eventHubLogger = CreateTestEventHubLogger(LogLevel.None, ukhoConfigLogLevel, "UKHO.SomeNamespace.SomeClass", fakeEventHubLog);

            eventHubLogger.Log<object>(attemptedLogLevel, 0, null, null, null);

            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).MustNotHaveHappened();
        }

        [TestCase(LogLevel.None, LogLevel.Critical, LogLevel.Critical, "UKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.None, LogLevel.Warning, LogLevel.Critical, "UKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.None, LogLevel.Information, LogLevel.Warning, "UKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.None, LogLevel.Trace, LogLevel.Information, "UKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.Critical, LogLevel.None, LogLevel.Critical, "NonUKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.Warning, LogLevel.None, LogLevel.Critical, "NonUKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.Information, LogLevel.None, LogLevel.Warning, "NonUKHO.SomeNamespace.SomeClass")]
        [TestCase(LogLevel.Trace, LogLevel.None, LogLevel.Information, "NonUKHO.SomeNamespace.SomeClass")]
        public void EventHubLoggerLogsCorrectLogEntryWhenAppropriate(LogLevel configLogLevel, LogLevel ukhoConfigLogLevel, LogLevel attemptedLogLevel, string category)
        {
            var environment = "Test Environment";
            var myCorrelationId = "MyCorrelationId";
            var testMessage = "Test message";
            var state = new object();
            var loggedException = new Exception();
            var eventId = (EventId)12345;
            var myUser = "MyUser";
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);

            var eventHubLogger = CreateTestEventHubLogger(configLogLevel,
                                                          ukhoConfigLogLevel,
                                                          category,
                                                          fakeEventHubLog,
                                                          d =>
                                                          {
                                                              d.Add("_CorrelationId", myCorrelationId);
                                                              d.Add("_Username", myUser);
                                                          });
            eventHubLogger.Log(attemptedLogLevel, eventId, state, loggedException, (o, exception) => testMessage);

            Assert.AreEqual(environment, loggedEntry.LogProperties["_Environment"]);
            Assert.AreEqual(myCorrelationId, loggedEntry.LogProperties["_CorrelationId"]);
            Assert.AreEqual(testMessage, loggedEntry.MessageTemplate);
            Assert.AreEqual(category, loggedEntry.LogProperties["_ComponentName"]);
            Assert.AreEqual(loggedException, loggedEntry.Exception);
            Assert.AreEqual(Service, loggedEntry.LogProperties["_Service"]);
            Assert.AreEqual(eventId, loggedEntry.EventId);
            Assert.AreEqual(myUser, loggedEntry.LogProperties["_Username"]);
        }

        [TestCase(LogLevel.None, LogLevel.Critical, LogLevel.Critical, "UKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.None, LogLevel.Warning, LogLevel.Critical, "UKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.None, LogLevel.Information, LogLevel.Warning, "UKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.None, LogLevel.Trace, LogLevel.Information, "UKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.Critical, LogLevel.None, LogLevel.Critical, "NonUKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.Warning, LogLevel.None, LogLevel.Critical, "NonUKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.Information, LogLevel.None, LogLevel.Warning, "NonUKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.Trace, LogLevel.None, LogLevel.Information, "NonUKHO.SomeNamespace.SomeClass", true)]
        [TestCase(LogLevel.None, LogLevel.Critical, LogLevel.Warning, "UKHO.SomeNamespace.SomeClass", false)]
        [TestCase(LogLevel.None, LogLevel.Warning, LogLevel.Information, "UKHO.SomeNamespace.SomeClass", false)]
        [TestCase(LogLevel.None, LogLevel.Information, LogLevel.Trace, "UKHO.SomeNamespace.SomeClass", false)]
        [TestCase(LogLevel.Critical, LogLevel.None, LogLevel.Warning, "NonUKHO.SomeNamespace.SomeClass", false)]
        [TestCase(LogLevel.Warning, LogLevel.None, LogLevel.Information, "NonUKHO.SomeNamespace.SomeClass", false)]
        [TestCase(LogLevel.Information, LogLevel.None, LogLevel.Trace, "NonUKHO.SomeNamespace.SomeClass", false)]
        public void IsEnabledReturnsCorrectResult(LogLevel configLogLevel, LogLevel ukhoConfigLogLevel, LogLevel attemptedLogLevel, string category, bool expectedResult)
        {
            var eventHubLogger = CreateTestEventHubLogger(configLogLevel, ukhoConfigLogLevel, category, fakeEventHubLog);

            Assert.AreEqual(expectedResult, eventHubLogger.IsEnabled(attemptedLogLevel));
        }

        [Test]
        public void TestLoggerLogsRequiredEnvironmentParameters()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);

            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog);
            eventHubLogger.Log(LogLevel.Information, 123, "Simple Message", null, (s, e) => s);

            CollectionAssert.AreEquivalent(new[]
                                           {
                                               "_ComponentName",
                                               "_Environment",
                                               "_NodeName",
                                               "_Service",
                                               "_System",
                                           }.ToList(),
                                           loggedEntry.LogProperties.Keys);
        }

        [Test]
        public void TestLoggerLogsAllTheIndividualParamsForStructuredData()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);

            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog);

            IEnumerable<KeyValuePair<string, object>> structuredData = new Dictionary<string, object> { { "Property1", "Value1" }, { "Property2", "Value Two" }, };
            eventHubLogger.Log(LogLevel.Information, 123, structuredData, null, (s, e) => string.Join(", ", s.Select(kv => $"{kv.Key}:{kv.Value}")));

            CollectionAssert.DoesNotContain(loggedEntry.LogProperties.Keys, "MessageTemplate");
            CollectionAssert.Contains(loggedEntry.LogProperties.Keys, "Property1");
            CollectionAssert.Contains(loggedEntry.LogProperties.Keys, "Property2");

            Assert.AreEqual("Value1", loggedEntry.LogProperties["Property1"]);
            Assert.AreEqual("Value Two", loggedEntry.LogProperties["Property2"]);
            Assert.AreEqual("Property1:Value1, Property2:Value Two", loggedEntry.MessageTemplate);
        }

        [Test]
        public void TestLoggerLogsAllTheIndividualParamsForStructuredDataUsingFormattedLogValues()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);

            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog);
            IEnumerable<KeyValuePair<string, object>> structuredData =
                new FormattedLogValues("Message with {Property1} and {Property2} and a escaped C# keyword name {@var}", "Value 1", "Value 2", "Var value");
            eventHubLogger.Log(LogLevel.Information, 123, structuredData, null, (s, e) => string.Join(",", s.Select(kv => $"{kv.Key}:{kv.Value}")));

            CollectionAssert.DoesNotContain(loggedEntry.LogProperties.Keys, "MessageTemplate");
            CollectionAssert.Contains(loggedEntry.LogProperties.Keys, "Property1");
            Assert.AreEqual("Value 1", loggedEntry.LogProperties["Property1"]);
            Assert.AreEqual("Value 2", loggedEntry.LogProperties["Property2"]);
            Assert.AreEqual("Var value", loggedEntry.LogProperties["var"]);
            Assert.AreEqual("Message with {Property1} and {Property2} and a escaped C# keyword name {@var}", loggedEntry.MessageTemplate);
        }

        [Test]
        public void TestLoggerLogsAllTheIndividualParamsForStructuredDataUsingFormattedLogValuesWithDuplicateParameterNames()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);

            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog);
            IEnumerable<KeyValuePair<string, object>> structuredData =
                new FormattedLogValues("Message with {Property1} and {Property1} and a escaped C# keyword name {@var}", "Value 1", "Value 2", "Var value");
            eventHubLogger.Log(LogLevel.Information, 123, structuredData, null, (s, e) => string.Join(",", s.Select(kv => $"{kv.Key}:{kv.Value}")));

            CollectionAssert.DoesNotContain(loggedEntry.LogProperties.Keys, "MessageTemplate");
            CollectionAssert.Contains(loggedEntry.LogProperties.Keys, "Property1");
            CollectionAssert.AreEqual(new[] { "Value 1", "Value 2" }.ToList(), loggedEntry.LogProperties["Property1"] as IEnumerable);
            Assert.AreEqual("Var value", loggedEntry.LogProperties["var"]);
            Assert.AreEqual("Message with {Property1} and {Property1} and a escaped C# keyword name {@var}", loggedEntry.MessageTemplate);
        }

        [Test]
        public void TestLoggerAddsAdditionalParameters()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);
            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog, d => d["AdditionalData"] = "NewData");
            eventHubLogger.Log(LogLevel.Error, 456, "Log Info", null, (s, e) => s);
            Assert.AreEqual("NewData", loggedEntry.LogProperties["AdditionalData"]);
        }

        [Test]
        public void TestLoggerAddExceptionToLogIfAdditionalParametersActionExplodes()
        {
            LogEntry loggedEntry = null;
            A.CallTo(() => fakeEventHubLog.Log(A<LogEntry>.Ignored)).Invokes((LogEntry l) => loggedEntry = l);
            var exception = new Exception("My Exception Message");
            var eventHubLogger = CreateTestEventHubLogger(LogLevel.Information, LogLevel.Information, "UKHO.TestClass", fakeEventHubLog, d => throw exception);
            eventHubLogger.Log(LogLevel.Error, 456, "Log Info", null, (s, e) => s);
            Assert.IsFalse(loggedEntry.LogProperties.ContainsValue("AdditionalData"));
            Assert.AreEqual("additionalValuesProvider throw exception: My Exception Message", loggedEntry.LogProperties["LoggingError"]);
            Assert.AreSame(exception, loggedEntry.LogProperties["LoggingErrorException"]);
        }

        private EventHubLogger CreateTestEventHubLogger(LogLevel configLogLevel,
                                                        LogLevel ukhoLogLevel,
                                                        string category,
                                                        IEventHubLog eventHubLog,
                                                        Action<IDictionary<string, object>> additionalValuesProvider = null)
        {
            return new EventHubLogger(eventHubLog,
                                      category,
                                      new EventHubLogProviderOptions
                                      {
                                          DefaultMinimumLogLevel = configLogLevel,
                                          NodeName = NodeName,
                                          System = System,
                                          Environment = "Test Environment",
                                          Service = Service,
                                          AdditionalValuesProvider = additionalValuesProvider ?? (d => { }),
                                          MinimumLogLevels = { { "UKHO", ukhoLogLevel } }
                                      });
        }

        
    }
}