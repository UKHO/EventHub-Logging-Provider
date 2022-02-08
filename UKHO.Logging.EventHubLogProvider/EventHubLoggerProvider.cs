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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces;

namespace UKHO.Logging.EventHubLogProvider
{
    internal class EventHubLoggerProvider : ILoggerProvider
    {
        private readonly EventHubLogProviderOptions options;
        private readonly IEventHubLog eventHubLog;
        private bool disposed;

        internal EventHubLoggerProvider(EventHubLogProviderOptions options,
                                        IEventHubLog eventHubLog)
        {
            this.options = options;
            this.eventHubLog = eventHubLog;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            eventHubLog?.Dispose();
            disposed = true;
        }

        ~EventHubLoggerProvider()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new EventHubLogger(eventHubLog, categoryName, options);
        }
    }

    [ExcludeFromCodeCoverage] // this is not testable due to AddProvider being a Microsoft extension method
    public static class EventHubLoggerProviderExtensions
    {
        public static ILoggerFactory AddEventHub(this ILoggerFactory loggerFactory, Action<EventHubLogProviderOptions> config)
        {
            var options = new EventHubLogProviderOptions();
            config(options);
            options.Validate();
            loggerFactory.AddProvider(new EventHubLoggerProvider(options,
                                                                 new EventHubLog(new EventHubClientWrapper(options.EventHubConnectionString, options.EventHubEntityPath,options.AzureStorageLogProviderOptions))));
            return loggerFactory;
        }

        [ExcludeFromCodeCoverage] // this is not testable due to AddProvider being a Microsoft extension method
        public static ILoggingBuilder AddEventHub(this ILoggingBuilder loggingBuilder, Action<EventHubLogProviderOptions> config)
        {
            var options = new EventHubLogProviderOptions();
            config(options);
            options.Validate();
            loggingBuilder.AddProvider(new EventHubLoggerProvider(options,
                                                                 new EventHubLog(new EventHubClientWrapper(options.EventHubConnectionString, options.EventHubEntityPath,options.AzureStorageLogProviderOptions))));
            return loggingBuilder;
        }
    }
}