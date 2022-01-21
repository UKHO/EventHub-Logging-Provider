using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  System.IO;
using System.Threading.Tasks;

namespace UKHO.Logging.EventHubLogProvider.Extensions
{
    public static class AzureStorageEventLoggerExtensions
    {
        /// <summary>
        /// Determines is a string is greater or equal to the  size
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="mbs">The size in MBs</param>
        /// <returns>True/False</returns>
        public static bool IsLongMessage(this string message,int mbs)
        {
            int messageSize = Encoding.UTF8.GetByteCount(message);

            return (messageSize >= (mbs * (1024^2)));
 

        }
    }
}
