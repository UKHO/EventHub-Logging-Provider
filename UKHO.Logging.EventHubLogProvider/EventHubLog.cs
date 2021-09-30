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
using System.Text;

using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace UKHO.Logging.EventHubLogProvider
{
    internal class EventHubLog : IEventHubLog
    {
        private IEventHubClientWrapper eventHubClientWrapper;
        private readonly JsonSerializerSettings settings;

        public EventHubLog(IEventHubClientWrapper eventHubClientWrapper)
        {
            this.eventHubClientWrapper = eventHubClientWrapper;
            settings = new JsonSerializerSettings
                       {
                           Formatting = Formatting.Indented,
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                           ContractResolver = new NullPropertyResolver()
                       };
        }

        public async void Log(LogEntry logEntry)
        {
            try
            {
                string jsonLogEntry;
                try
                {
                    jsonLogEntry = JsonConvert.SerializeObject(logEntry, settings);
                }
                catch (Exception e)
                {
                    logEntry = new LogEntry()
                               {
                                   Exception = e,
                                   Level = "Warning",
                                   MessageTemplate = "Log Serialization failed with exception",
                                   Timestamp = DateTime.UtcNow,
                                   EventId = new EventId(7437)
                               };
                    jsonLogEntry = JsonConvert.SerializeObject(logEntry, settings);
                }

                await eventHubClientWrapper.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonLogEntry)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReleaseUnmanagedResources()
        {
            eventHubClientWrapper?.Dispose();
            eventHubClientWrapper = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~EventHubLog()
        {
            ReleaseUnmanagedResources();
        }
    }
}