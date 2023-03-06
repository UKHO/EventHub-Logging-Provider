// British Crown Copyright © 2023,
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
using System.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

using SystemEnvironment = System.Environment;

namespace UKHO.Logging.EventHubLogProvider
{
    public class EventHubLogProviderOptions
    {
        private const string HashMachineName = "#MachineName";
        private string nodeName = HashMachineName;
        public string EventHubConnectionString { get; set; }
        public string EventHubEntityPath { get; set; }
        public LogLevel DefaultMinimumLogLevel { get; set; } = LogLevel.Information;
        public IDictionary<string, LogLevel> MinimumLogLevels { get; } = new Dictionary<string, LogLevel>();
        public string Environment { get; set; }
        public string System { get; set; }
        public Action<IDictionary<string, object>> AdditionalValuesProvider { get; set; } = d => { };
        public AzureStorageLogProviderOptions AzureStorageLogProviderOptions { get; set; }

        /// <summary>
        ///     If set to true, the configuration will be actively validated with EventHub and will throw an ArgumentException if the
        ///     connection with EventHub can't be established and validated
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool ValidateConnectionString { get; set; } = false;

        /// <summary>
        ///     If set to "#MachineName", this will resolve to SystemEnvironment.MachineName at runtime.
        /// </summary>
        public string NodeName
        {
            get => nodeName == HashMachineName ? SystemEnvironment.MachineName : nodeName;
            set => nodeName = value;
        }

        public string Service { get; set; }

        /// <summary>
        ///     Generally, this property is optional and not required. These converters get applied to the log object serialization
        ///     before the log is sent to EventHub. This allows customisation of the serialization of specific log parameter objects
        ///     if required. For example, it maybe desirable to serialize an object's properties as a single string rather than
        ///     individual properties. In the case of the Version object, 1.2.3.4 is sufficient and do not require the properties
        ///     to be individual separated. Similarly, it maybe desirable to ommit some properties of some objects, so a serializer
        ///     could be created that only includes required properties.
        /// 
        ///     These converters must be able to write (WriteJson must be fully implemented and CanWrite must return true). It is
        ///     not necessary for the convert to implement ReadJson and CanRead may return false.
        /// </summary>
        public IEnumerable<JsonConverter> CustomLogSerializerConverters { get; set; } = new List<JsonConverter>();

        public void Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(EventHubConnectionString))
                errors.Add(nameof(EventHubConnectionString));
            if (string.IsNullOrEmpty(EventHubEntityPath))
                errors.Add(nameof(EventHubEntityPath));
            if (string.IsNullOrEmpty(Environment))
                errors.Add(nameof(Environment));
            if (string.IsNullOrEmpty(System))
                errors.Add(nameof(System));
            if (string.IsNullOrEmpty(Service))
                errors.Add(nameof(Service));
            if (string.IsNullOrEmpty(NodeName))
                errors.Add(nameof(NodeName));
            if (AdditionalValuesProvider == null)
                errors.Add(nameof(AdditionalValuesProvider));

            if (errors.Any())
                throw new ArgumentException($"Parameters {string.Join(",", errors)} must be set to a valid value.", string.Join(",", errors));

            if (MinimumLogLevels.ContainsKey(""))
                throw new ArgumentException($"Parameter {nameof(MinimumLogLevels)} can not contain an empty key.", nameof(MinimumLogLevels));


            if (CustomLogSerializerConverters==null)
                throw new ArgumentNullException(nameof(CustomLogSerializerConverters), $"Parameter {nameof(CustomLogSerializerConverters)} can not be null.");
            
            if (CustomLogSerializerConverters.Any(c => c == null))
                throw new ArgumentNullException(nameof(CustomLogSerializerConverters), $"Parameter {nameof(CustomLogSerializerConverters)} can not contain null entries.");
            var badConverters = CustomLogSerializerConverters.Where(s => !s.CanWrite).ToList();
            if (badConverters.Any())
            {
                throw new ArgumentException($"{nameof(CustomLogSerializerConverters)} must be able to write: {string.Join(",", badConverters.Select(c => c?.GetType().FullName??"null"))}");
            }

            if (ValidateConnectionString)
                ValidateConnection();
        }

        private void ValidateConnection()
        {
            var eventHubClientWrapper = new EventHubClientWrapper(EventHubConnectionString, EventHubEntityPath, AzureStorageLogProviderOptions);
            eventHubClientWrapper.ValidateConnection();
        }

        public LogLevel GetMinimumLogLevelForCategory(string categoryName)
        {
            var categoryTokens = SplitKey(categoryName);
            var configuration = MinimumLogLevels
                .Select(kv => new KeyValuePair<string[], LogLevel>(SplitKey(kv.Key), kv.Value))
                .Where(kv =>
                       {
                           return kv.Key.Select((k, i) => (k, i))
                               .All(k => k.Item2 < categoryTokens.Length && k.Item1 == categoryTokens[k.Item2]);
                       })
                .OrderByDescending(kv => kv.Key.Length)
                .Select(kv => kv.Value)
                .DefaultIfEmpty(DefaultMinimumLogLevel)
                .FirstOrDefault();
            return configuration;
        }

        private static string[] SplitKey(string key)
        {
            return key.Split('.').SelectMany(k => k.Split('\\')).ToArray();
        }
    }
}