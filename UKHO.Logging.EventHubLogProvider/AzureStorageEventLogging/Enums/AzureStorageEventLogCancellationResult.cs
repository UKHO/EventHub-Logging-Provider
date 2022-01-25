using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums
{
    public enum AzureStorageEventLogCancellationResult
    {
        Successful = 0,
        UnableToCancel = 1,
        CancellationFailed = 2
    }
}
