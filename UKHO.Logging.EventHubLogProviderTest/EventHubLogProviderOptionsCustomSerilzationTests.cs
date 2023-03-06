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

using Newtonsoft.Json;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider;

namespace UKHO.Logging.EventHubLogProviderTest
{
    public class EventHubLogProviderOptionsCustomSerializationTests
    {
        [Test]
        public void TestSimpleConfigurationOfValidSerializationCustomizations()
        {
            var options = new EventHubLogProviderOptions
            {
                Environment = "Env",
                EventHubConnectionString = "Connect!",
                EventHubEntityPath = "Path",
                Service = "Service",
                System = "System",
                CustomLogSerializerConverters = new List<JsonConverter> { new VersionJsonConverter() }
            };
            options.Validate();
        }

        [Test]
        public void TestNullCustomLogSerializerConverters()
        {
            var options = new EventHubLogProviderOptions
            {
                Environment = "Env",
                EventHubConnectionString = "Connect!",
                EventHubEntityPath = "Path",
                Service = "Service",
                System = "System",
                CustomLogSerializerConverters = null
            };
            var thrownException = Assert.Throws<ArgumentNullException>(() => options.Validate());
            StringAssert.StartsWith("Parameter CustomLogSerializerConverters can not be null.", thrownException.Message);
        }

        [Test]
        public void TestNullCustomLogSerializerConverter()
        {
            var options = new EventHubLogProviderOptions
            {
                Environment = "Env",
                EventHubConnectionString = "Connect!",
                EventHubEntityPath = "Path",
                Service = "Service",
                System = "System",
                CustomLogSerializerConverters = new List<JsonConverter> { null }
            };
            var thrownException = Assert.Throws<ArgumentNullException>(() => options.Validate());
            StringAssert.Contains("Parameter CustomLogSerializerConverters can not contain null entries.", thrownException.Message);
        }

        [Test]
        public void TestSerializationCustomizationsRequiresConvertersThatCanWrite()
        {
            var options = new EventHubLogProviderOptions
            {
                Environment = "Env",
                EventHubConnectionString = "Connect!",
                EventHubEntityPath = "Path",
                Service = "Service",
                System = "System",
                CustomLogSerializerConverters = new List<JsonConverter> { new ReadonlyJsonConverter() }
            };
            var thrownException = Assert.Throws<ArgumentException>(() => options.Validate());
            Assert.AreEqual("CustomLogSerializerConverters must be able to write: UKHO.Logging.EventHubLogProviderTest.ReadonlyJsonConverter", thrownException.Message);
        }
    }

    internal class ReadonlyJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;
    }
}