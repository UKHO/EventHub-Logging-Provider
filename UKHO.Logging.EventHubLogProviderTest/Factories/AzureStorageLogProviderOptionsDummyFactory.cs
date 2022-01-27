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
            return new AzureStorageLogProviderOptions("http://test.com/test/", true, "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}", "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}");
        }
    }
}