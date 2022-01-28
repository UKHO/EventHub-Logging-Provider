using System;
using System.Configuration;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    /// <summary>
    ///     The resources factory model
    /// </summary>
    public class ResourcesFactory
    {
        /// <summary>
        ///     The default constructor
        /// </summary>
        public ResourcesFactory()
        {
            SuccessTemplateMessage =
                "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}";
                //GetSetting("AzureStorage.SuccessMessageTemplate");
                FailureTemplateMessage =
                    "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}";
                //GetSetting("AzureStorage.FailureMessageTemplate");
        }

        public string SuccessTemplateMessage { get; }
        public string FailureTemplateMessage { get; }

        // <summary>
        /// Returns the setting's value for the supplied key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        private static string GetSetting(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);

            return string.IsNullOrEmpty(value) ? throw new ArgumentNullException(key) : value;
        }
    }
}