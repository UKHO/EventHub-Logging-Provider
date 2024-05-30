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
        private EventHubLogProviderOptions options;

        [SetUp]
        public void SetUp()
        {
            options = new EventHubLogProviderOptions
                      {
                          Environment = "Env",
                          EventHubEntityPath = "Path",
                          Service = "Service",
                          System = "System"
                      };
        }

        [Test]
        public void TestValidateWithDefaultsFailsValidation()
        {
            //Arrange
            const string parameterNames = "EventHubConnectionString,EventHubEntityPath,Environment,System,Service";
            options = new EventHubLogProviderOptions();

            //Act
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());

            //Assert
            Assert.NotNull(argumentException);
            Assert.AreEqual(parameterNames, argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith($"Parameters {parameterNames} must be set to a valid value"));
        }

        [Test]
        public void TestValidateOkWithMinimumValuesSet()
        {
            //Arrange
            options.EventHubConnectionString = "Connect!";

            //Act
            //Assert
            Assert.DoesNotThrow(() => options.Validate());
        }

        [Test]
        public void TestValidateOkWithAllValuesSet()
        {
            //Arrange
            options.EventHubConnectionString = "Connect!";
            options.DefaultMinimumLogLevel = LogLevel.Critical;
            options.NodeName = "Bill";

            //Act
            //Assert
            Assert.DoesNotThrow(() => options.Validate());
        }

        [Test]
        public void TestValidateConnectionExplodesWithAnArgumentExceptionIfConnectionStringRubbishAndValidateConnectionStringTurnedOn()
        {
            //Arrange
            options.EventHubConnectionString =
                "Endpoint=sb://abadname-eventhubnamespace.servicebus.windows.net/;SharedAccessKeyName=logstash;SharedAccessKey=garbage=;EntityPath=eventhub";
            options.DefaultMinimumLogLevel = LogLevel.Critical;
            options.NodeName = "Bill";
            options.ValidateConnectionString = true;

            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => options.Validate());
        }

        [Test]
        public void TestValidateFailsWithAnEmptyNamespaceLogLevelOverride()
        {
            options.EventHubConnectionString = "Connect!";
            options.MinimumLogLevels.Add("", LogLevel.Critical);

            //Act
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());

            //Assert
            Assert.NotNull(argumentException);
            Assert.AreEqual("MinimumLogLevels", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameter MinimumLogLevels can not contain an empty key"));
        }

        [Test]
        public void TestValidateFailsWhenAdditionalValuesActionSetToNull()
        {
            //Arrange
            options.EventHubConnectionString = "Connect!";
            options.AdditionalValuesProvider = null;

            //Act
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());

            //Assert
            Assert.NotNull(argumentException);
            Assert.AreEqual("AdditionalValuesProvider", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameters AdditionalValuesProvider must be set to a valid value"));
        }

        [Test]
        public void TestValidateWithAllValuesSetButNodeNameUnsetWillThrowExceptionForTheNodeName()
        {
            //Arrange
            options.EventHubConnectionString = "Connect!";
            options.DefaultMinimumLogLevel = LogLevel.Critical;
            options.NodeName = "";

            //Act
            var argumentException = Assert.Throws<ArgumentException>(() => options.Validate());

            //Assert
            Assert.NotNull(argumentException);
            Assert.AreEqual("NodeName", argumentException.ParamName);
            Assert.That(argumentException.Message, Does.StartWith("Parameters NodeName must be set to a valid value"));
        }

        [Test]
        public void TestMachineNameMacroOnGet()
        {
            var myComputerName = Environment.MachineName;
            var eventHubLogProviderOptions = new EventHubLogProviderOptions();
            Assert.AreEqual(myComputerName, eventHubLogProviderOptions.NodeName);

            eventHubLogProviderOptions.NodeName = "ExplicitValue";
            Assert.AreEqual("ExplicitValue", eventHubLogProviderOptions.NodeName);
        }

        [Test]
        public void TestConfigureLoggingLevelsForDifferentNamespaces()
        {
            options.DefaultMinimumLogLevel = LogLevel.Critical;
            options.MinimumLogLevels.Add("UKHO", LogLevel.Trace);
            options.MinimumLogLevels.Add("UKHO.Logging", LogLevel.Information);
            options.MinimumLogLevels.Add("UKHO.Logging.Event", LogLevel.None);
            options.MinimumLogLevels.Add("UKHO.Security", LogLevel.Debug);
            options.MinimumLogLevels.Add("AnImportant\\Path", LogLevel.Warning);

            Assert.Multiple(() =>
                            {
                                Assert.AreEqual(LogLevel.Information, options.GetMinimumLogLevelForCategory("UKHO.Logging.EventLogger"));
                                Assert.AreEqual(LogLevel.Debug, options.GetMinimumLogLevelForCategory("UKHO.Security.Authentication.AnAuthenticator"));
                                Assert.AreEqual(LogLevel.Trace, options.GetMinimumLogLevelForCategory("UKHO.Controllers.AController"));
                                Assert.AreEqual(LogLevel.Critical, options.GetMinimumLogLevelForCategory("System.SomeSystemClass"));
                                Assert.AreEqual(LogLevel.Critical, options.GetMinimumLogLevelForCategory("ARandomPath\\With\\Some\\Folders"));
                                Assert.AreEqual(LogLevel.Warning, options.GetMinimumLogLevelForCategory("AnImportant\\Path"));
                                Assert.AreEqual(LogLevel.Warning, options.GetMinimumLogLevelForCategory("AnImportant\\Path\\WithChild"));
                            });
        }
    }
}