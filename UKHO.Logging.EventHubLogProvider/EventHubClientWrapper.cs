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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace UKHO.Logging.EventHubLogProvider
{
    //Wrapper for the external library so we can test things.
    internal interface IEventHubClientWrapper : IDisposable
    {
        Task SendAsync(EventData eventData);
    }

    [ExcludeFromCodeCoverage] // not testable as it's just a wrapper for EventHubClient
    internal class EventHubClientWrapper : IEventHubClientWrapper
    {
        private EventHubProducerClient eventHubClient;

        public EventHubClientWrapper(string eventHubConnectionString, string eventHubEntityPath)
        {
            eventHubClient = new EventHubProducerClient(eventHubConnectionString, eventHubEntityPath);
            //TODO: fixed!
            //var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
            //                              {
            //                                  EntityPath = eventHubEntityPath
            //                              };

            //eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        private void ReleaseUnmanagedResources()
        {
            eventHubClient.CloseAsync().Wait();
            //TODO: fixed!
            //eventHubClient?.Close();
            eventHubClient = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~EventHubClientWrapper()
        {
            ReleaseUnmanagedResources();
        }

        public Task SendAsync(EventData eventData)
        {
            return eventHubClient.SendAsync(new List<EventData>{ eventData });
            //TODO: fixed!
            //return eventHubClient.SendAsync(eventData);
        }

        public void ValidateConnection()
        {
            try
            {
                eventHubClient.GetPartitionIdsAsync().Wait();
                //TODO: fixed!
                //eventHubClient.GetRuntimeInformationAsync().Wait();
            }
            catch (AggregateException e)
            {
                throw new ArgumentException("The connection to EventHub failed.", e);
            }
        }
    }
}