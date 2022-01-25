// British Crown Copyright © 2022,
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

using UKHO.Logging.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Enums;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces
{
    public interface IAzureStorageEventLogger
    {
        /// <summary>
        /// Generates the service name for the container subfolder
        /// </summary>
        /// <param name="serviceName">The service name</param>
        /// <param name="environment">The environment</param>
        /// <returns>the service name(string)</returns>
        string GenerateServiceName(string serviceName,string environment);

        /// <summary>
        /// Generates a path for the error message blob
        /// </summary>
        /// <param name="date">The datetime</param>
        /// <returns>The blob path</returns>
        string GeneratePathForErrorBlob(DateTime date);

        /// <summary>
        /// Generates a new blob name
        /// </summary>
        /// <param name="name">The name of the blob</param>
        /// <param name="extension">The extension</param>
        /// <returns>The blob name(with extension)</returns>
        string GenerateErrorBlobName(Nullable<Guid> name = null, string extension = null);

        /// <summary>
        /// Generates the full name for the blob
        /// </summary>
        /// <param name="storageBlobFullNameModel">The blob full name model</param>
        /// <returns>The fullname of the blob (path + name)</returns>
        string GenerateBlobFullName(AzureStorageBlobFullNameModel storageBlobFullNameModel);

        /// <summary>
        /// Cancels the storing operation (when possible)
        /// </summary>
        /// <returns>AzureStorageEventLogCancellationResult</returns>
        AzureStorageEventLogCancellationResult CancelLogFileStoringOperation();

        AzureStorageEventLogResult StoreLogFile(AzureStorageEventModel model,bool withCancellation = false);
    }
}