using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    public class AzureStorageLogProviderOptionsDummyFactory : DummyFactory<AzureStorageLogProviderOptions>
    {


        protected override AzureStorageLogProviderOptions Create()
        {
            return new AzureStorageLogProviderOptions("http://test.com/test/", true);
        }
    }
}
