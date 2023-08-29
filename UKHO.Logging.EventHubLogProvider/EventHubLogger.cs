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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;

namespace UKHO.Logging.EventHubLogProvider
{
    internal class EventHubLogger : ILogger
    {
        private const string OriginalFormatPropertyName = "{OriginalFormat}";
        private readonly IEventHubLog eventHubLog;
        private readonly LogLevel minimumLogLevel;
        private readonly string environment;
        private readonly string categoryName;
        private readonly string system;
        private readonly string service;
        private readonly string nodeName;
        private readonly Action<IDictionary<string, object>> additionalValuesProvider;

        internal EventHubLogger(IEventHubLog eventHubLog,
                                string categoryName,
                                EventHubLogProviderOptions options)
        {
            this.eventHubLog = eventHubLog;
            this.categoryName = categoryName;
            minimumLogLevel = options.GetMinimumLogLevelForCategory(categoryName);
            environment = options.Environment;
            system = options.System;
            service = options.Service;
            nodeName = options.NodeName;
            additionalValuesProvider = options.AdditionalValuesProvider;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            (Dictionary<string, object> logProperties, string MessageTemplate) BuildLogProperties()
            {
                var result = new Dictionary<string, object>
                             {
                                 { "_Environment", environment },
                                 { "_System", system },
                                 { "_Service", service },
                                 { "_NodeName", nodeName },
                                 { "_ComponentName", categoryName },
                             };
                try
                {
                    additionalValuesProvider(result);
                }
                catch (Exception e)
                {
                    result["LoggingError"] = $"additionalValuesProvider throw exception: {e.Message}";
                    result["LoggingErrorException"] = e;
                }

                var messageTemplate = "";
                if (state is IEnumerable<KeyValuePair<string, object>> structure)
                {
                    foreach (var property in structure)
                    {
                        if (property.Key == OriginalFormatPropertyName && property.Value is string)
                        {
                            messageTemplate = (string)property.Value;
                        }
                        else
                        {
                            var propertyName = property.Key.StartsWith("@") ? property.Key.Substring(1) : property.Key;
                            if (result.ContainsKey(propertyName))
                            {
                                var existingValue = result[propertyName] as List<object> ?? new List<object>() { result[propertyName] };
                                existingValue.Add(property.Value);
                                result[propertyName] = existingValue;
                            }
                            else
                                result.Add(propertyName, property.Value);
                        }
                    }

                    if (string.IsNullOrEmpty(messageTemplate))
                    {
                        messageTemplate = formatter(state, exception);
                    }
                }
                else
                {
                    messageTemplate = formatter(state, exception);
                }

                return (result, messageTemplate);
            }

            if (!IsEnabled(logLevel))
                return;

            var logProperties = BuildLogProperties();
            var logEntry = new LogEntry
                           {
                               Exception = exception,
                               EventId = eventId,
                               Level = logLevel.ToString(),
                               MessageTemplate = logProperties.MessageTemplate,
                               Timestamp = DateTime.UtcNow,
                               LogProperties = logProperties.logProperties
                           };
            eventHubLog.Log(logEntry);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= minimumLogLevel;
        }

        [ExcludeFromCodeCoverage] // nothing to test
        public IDisposable BeginScope<TState>(TState state)
        {
            return new EmptyDisposable();
        }
    }
}