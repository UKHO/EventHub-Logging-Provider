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

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class EventHubLogProviderOptionsTests
    {
        [Test]
        public void TestValidateWithDefaultsFailsValidation()
        {
            var options = new EventHubLogProviderOptions();
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());
            var parameterNames = "EventHubConnectionString,EventHubEntityPath,Environment,System,Service";
            Assert.AreEqual(parameterNames, argumentException.ParamName);
            Assert.AreEqual($"Parameters {parameterNames} must be set to a valid value.{Environment.NewLine}Parameter name: {parameterNames}", argumentException.Message);
        }

        [Test]
        public void TestValidateOkWithMinimumValuesSet()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System"
                          };
            options.Validate();
        }

        [Test]
        public void TestValidateOkWithAllValuesSet()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              MinimumLogLevel = LogLevel.Critical,
                              UkhoMinimumLogLevel = LogLevel.Critical,
                              NodeName = "Bill"
                          };
            options.Validate();
        }

        [Test]
        public void TestValidateOkWithAllValuesSetButNodeNameUnset()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              MinimumLogLevel = LogLevel.Critical,
                              UkhoMinimumLogLevel = LogLevel.Critical,
                              NodeName = ""
                          };
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.AreEqual("NodeName", argumentException.ParamName);
            Assert.AreEqual($"Parameters NodeName must be set to a valid value.{Environment.NewLine}Parameter name: NodeName", argumentException.Message);
        }

        [Test]
        public void TestMachineNameMacroOnGet()
        {
            var myComputerName = Environment.MachineName;
            var eventHubLogProviderOptions = new EventHubLogProviderOptions();
            Assert.AreEqual(myComputerName, eventHubLogProviderOptions.NodeName);

            eventHubLogProviderOptions.NodeName = "ExplictValue";
            Assert.AreEqual("ExplictValue", eventHubLogProviderOptions.NodeName);
        }
    }
}