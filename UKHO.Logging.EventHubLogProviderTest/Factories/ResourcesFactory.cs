using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace UKHO.Logging.EventHubLogProviderTest.Factories
{
    /// <summary>
    /// The resources factory model
    /// </summary>
    public class ResourcesFactory
    {
        public string SuccessTemplateMessage { get; private set; }
        public string FailureTemplateMessage { get; private set; }

        /// <summary>
        /// The default constructor
        /// </summary>
        public ResourcesFactory()
        {
            this.SuccessTemplateMessage = GetSetting("AzureStorage.SuccessMessageTemplate");
            this.FailureTemplateMessage = GetSetting("AzureStorage.FailureMessageTemplate");
        }


        // <summary>
        /// Returns the setting's value for the supplied key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value</returns>
        private static string GetSetting(string key)
        {
           string value = ConfigurationManager.AppSettings.Get(key);

            return string.IsNullOrEmpty(value) ? throw new ArgumentNullException(key) : value;
        }
    }
}
