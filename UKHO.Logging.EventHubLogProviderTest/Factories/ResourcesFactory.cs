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
            SuccessTemplateMessage = GetSetting("AzureStorage.SuccessMessageTemplate");
            FailureTemplateMessage = GetSetting("AzureStorage.FailureMessageTemplate");
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