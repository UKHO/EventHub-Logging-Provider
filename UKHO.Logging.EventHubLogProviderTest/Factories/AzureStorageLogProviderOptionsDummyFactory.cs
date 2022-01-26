using FakeItEasy;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    /// <summary>
    ///     Dummy factory for the azure storage log provider options
    /// </summary>
    public class AzureStorageLogProviderOptionsDummyFactory : DummyFactory<AzureStorageLogProviderOptions>
    {
        /// <summary>
        ///     Creates and returns an azure storage log provider options model (fake)
        /// </summary>
        /// <returns>A new AzureStorageLogProviderOptions model (fake)</returns>
        protected override AzureStorageLogProviderOptions Create()
        {
            return new AzureStorageLogProviderOptions("http://test.com/test/", true);
        }
    }
}