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
using System.Text;

using Microsoft.Azure.EventHubs;

using Newtonsoft.Json;

namespace UKHO.Logging.EventHubLogProvider
{
    [ExcludeFromCodeCoverage] // not testable as it's just a wrapper for EventHubClient
    public class EventHubLog : IEventHubLog
    {
        private EventHubClient eventHubClient;
        private readonly JsonSerializerSettings settings;

        public EventHubLog(string eventHubConnectionString, string eventHubEntityPath)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
                                          {
                                              EntityPath = eventHubEntityPath
                                          };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            settings = new JsonSerializerSettings
                       {
                           Formatting = Formatting.Indented
                       };
        }

        public async void Log(LogEntry logEntry)
        {
            try
            {
                var jsonLogEntry = JsonConvert.SerializeObject(logEntry, settings);
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonLogEntry)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReleaseUnmanagedResources()
        {
            eventHubClient?.Close();
            eventHubClient = null;
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