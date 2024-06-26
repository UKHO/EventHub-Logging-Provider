﻿// British Crown Copyright © 2022,
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

using Azure.Core;

namespace UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Interfaces
{
    /// <summary>
    /// The azure storage log provider options interface
    /// </summary>
    public interface IAzureStorageLogProviderOptions
    {
        /// <summary>
        ///     The azure storage sas url string
        /// </summary>
        string AzureStorageContainerSasUrlString { get; }

        /// <summary>
        ///     The azure storage blob container uri
        /// </summary>
        Uri AzureStorageBlobContainerUri { get; }

        /// <summary>
        ///     Gets a value indicating if Managed Identity authentication is configured.
        /// </summary>
        /// <returns>
        ///     `true` if the <see cref="EventHubFullyQualifiedNamespace"/> property has a value and Managed Identity should be used for authentication, otherwise `false`.
        /// </returns>
        bool IsUsingManagedIdentity { get; }

        /// <summary>
        ///      The <see cref="Azure.Core.TokenCredential"/> to use for authentication.
        /// </summary>
        TokenCredential AzureStorageCredential { get; }

        /// <summary>
        ///     Enable the azure storage functionality
        /// </summary>
        bool AzureStorageLoggerEnabled { get; }

        /// <summary>
        ///     The azure storage uri model
        /// </summary>
        Uri AzureStorageContainerSasUrl { get; }

        /// <summary>
        ///     The azure storage result successful template
        /// </summary>
        string SuccessfulMessageTemplate { get; }

        /// <summary>
        ///     The azure storage result failed template
        /// </summary>
        string FailedMessageTemplate { get; }
    }
}