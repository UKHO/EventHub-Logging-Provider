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
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class EventHubLoggerProviderTests
    {
        private EventHubLoggerProvider eventHubLogProvider;
        private readonly IEventHubLog fakeEventHubLog;

        public EventHubLoggerProviderTests()
        {
            fakeEventHubLog = A.Fake<IEventHubLog>();
            eventHubLogProvider = new EventHubLoggerProvider(new EventHubLogProviderOptions
                                                             {
                                                                 DefaultMinimumLogLevel = LogLevel.Critical,
                                                                 Environment = "Test Environment",
                                                                 System = "system",
                                                                 Service = "service",
                                                                 NodeName = "nodeName"
                                                             },
                                                             fakeEventHubLog);
        }

        [Test]
        public void EventHubLogGetsDisposedWithEventHubLogProvider()
        {
            eventHubLogProvider.Dispose();
            A.CallTo(() => fakeEventHubLog.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void EventDoesntGetDisposedWithEventHubLogProviderIfTheProviderHasAlreadyBeenDisposed()
        {
            eventHubLogProvider.Dispose();
            eventHubLogProvider.Dispose();
            A.CallTo(() => fakeEventHubLog.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void EventHubLogProviderCreatesILogger()
        {
            var logger = eventHubLogProvider.CreateLogger("My Log Category");
            Assert.IsInstanceOf<EventHubLogger>(logger);
        }

        [Test]
        public void EventHubLogProviderTestFinaliset()
        {
            eventHubLogProvider = null;

            GC.Collect(0, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            A.CallTo(() => fakeEventHubLog.Dispose()).MustHaveHappenedOnceExactly();
        }
    }
}