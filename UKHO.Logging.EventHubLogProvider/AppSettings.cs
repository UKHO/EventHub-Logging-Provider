using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;

namespace UKHO.Logging.EventHubLogProvider.Settings
{
    
        /// <summary>
        /// The settings model
        /// </summary>
        public class AppSettings
        {

            public string AzureStorageContainerSasUrl { get; private set; }
            
            /// <summary>
            /// The constructor
            /// </summary> 
            public AppSettings()
            { 
                this.AzureStorageContainerSasUrl = this.GetSetting("Logs.Queue.Container.SasUrl");
            }
            /// <summary>
            /// Returns the setting's value for the supplied key
            /// </summary>
            /// <param name="key">The key</param>
            /// <returns>The value</returns>
            public string GetSetting(string key)
            {
                return ConfigurationManager.AppSettings.Get(key);
            }

        }
    

}
