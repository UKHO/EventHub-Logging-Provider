// British Crown Copyright © 2019,
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
            Assert.That(argumentException.Message, Does.StartWith($"Parameters {parameterNames} must be set to a valid value"));
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
                              DefaultMinimumLogLevel = LogLevel.Critical,
                              NodeName = "Bill"
                          };
            options.Validate();
        }

        [Test]
        public void TestValidateConnectionExplodesWithAnArgumentExceptionIfConnectionStringRubbishAndValidateConnectionStringTurnedOn()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Endpoint=sb://abadname-eventhubnamespace.servicebus.windows.net/;SharedAccessKeyName=logstash;SharedAccessKey=garbage=;EntityPath=eventhub",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              DefaultMinimumLogLevel = LogLevel.Critical,
                              NodeName = "Bill",
                              ValidateConnectionString = true
                          };
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        [Test]
        public void TestValidateFailsWithAnEmptyNamespaceLogLevelOverride()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              MinimumLogLevels = { { "", LogLevel.Critical } }
                          };
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.AreEqual("MinimumLogLevels", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameter MinimumLogLevels can not contain an empty key"));
        }

        [Test]
        public void TestValidateFailsWhenAdditionalValuesActionSetToNull()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              AdditionalValuesProvider = null
                          };
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.AreEqual("AdditionalValuesProvider", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameters AdditionalValuesProvider must be set to a valid value"));
        }

        [Test]
        public void TestValidateWithAllValuesSetButNodeNameUnsetWillThrowExceptionForTheNodeName()
        {
            var options = new EventHubLogProviderOptions
                          {
                              Environment = "Env",
                              EventHubConnectionString = "Connect!",
                              EventHubEntityPath = "Path",
                              Service = "Service",
                              System = "System",
                              DefaultMinimumLogLevel = LogLevel.Critical,
                              NodeName = ""
                          };
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.AreEqual("NodeName", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameters NodeName must be set to a valid value"));
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

        [Test]
        public void TestConfigureLoggingLevelsForDifferentNamespaces()
        {
            var options = new EventHubLogProviderOptions
                          {
                              DefaultMinimumLogLevel = LogLevel.Critical,
                              MinimumLogLevels =
                              {
                                  ["UKHO"] = LogLevel.Trace,
                                  ["UKHO.Logging"] = LogLevel.Information,
                                  ["UKHO.Logging.Event"] = LogLevel.None,
                                  ["UKHO.Security"] = LogLevel.Debug,
                                  ["AnImportant\\Path"] = LogLevel.Warning
                              }
                          };

            Assert.AreEqual(LogLevel.Information, options.GetMinimumLogLevelForCategory("UKHO.Logging.EventLogger"));
            Assert.AreEqual(LogLevel.Debug, options.GetMinimumLogLevelForCategory("UKHO.Security.Authentication.AnAuthenticator"));
            Assert.AreEqual(LogLevel.Trace, options.GetMinimumLogLevelForCategory("UKHO.Controllers.AController"));
            Assert.AreEqual(LogLevel.Critical, options.GetMinimumLogLevelForCategory("System.SomeSystemClass"));
            Assert.AreEqual(LogLevel.Critical, options.GetMinimumLogLevelForCategory("ARandomPath\\With\\Some\\Folders"));
            Assert.AreEqual(LogLevel.Warning, options.GetMinimumLogLevelForCategory("AnImportant\\Path"));
            Assert.AreEqual(LogLevel.Warning, options.GetMinimumLogLevelForCategory("AnImportant\\Path\\WithChild"));
        }
    }
}