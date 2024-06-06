using System;
using Azure.Core;
using Moq;
using NUnit.Framework;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;
using UKHO.Logging.EventHubLogProviderTest.Factories;

namespace UKHO.Logging.EventHubLogProviderTest
{
    /// <summary>
    ///     Tests for the azure storage log provider
    /// </summary>
    [TestFixture]
    public class AzureStorageLogProviderOptionsTests
    {
        private readonly ResourcesFactory resourcesFactory = new ResourcesFactory();
        private const string _invalidUrl = "-invalidUrl";
        private const string _validUrl = "https://test.com/";

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is an invalid url string)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_InvalidUrl()
        {

            //Act
            var exception = Assert.Throws<UriFormatException>(() => new AzureStorageLogProviderOptions(_invalidUrl,
                                                                                                        true,
                                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                                        resourcesFactory.FailureTemplateMessage));

            //Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("Invalid sas url."));
        }

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is null)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_NullUrl()
        {
            string url = null;

            //Act
            var exception = Assert.Throws<NullReferenceException>(() => 
                                                                    new AzureStorageLogProviderOptions(url,
                                                                    true,
                                                                    resourcesFactory.SuccessTemplateMessage,
                                                                    resourcesFactory.FailureTemplateMessage));

            //Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("The Azure storage container sas url cannot be null or empty when Azure storage option is set to enabled"));
        }

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is a valid url string)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_ValidUrl()
        {
            var result = new AzureStorageLogProviderOptions(_validUrl, true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);

            Assert.IsNotNull(result.AzureStorageContainerSasUrl);
            Assert.AreEqual(result.AzureStorageContainerSasUrl.AbsoluteUri, _validUrl);
        }

        /// <summary>
        ///     Test for the method that validates the invalid blocb uri
        /// </summary>
        [Test]
        public void WhenUsingManagedIdentity_WithInvalidUri_ThrowsException()
        {
            //Arrange
            var tokenCredential = new Mock<TokenCredential>();
         
            //Act
            var exception = Assert.Throws<UriFormatException>(() => new AzureStorageLogProviderOptions(new Uri(_invalidUrl),
                                                                                                        tokenCredential.Object,
                                                                                                        true,
                                                                                                        resourcesFactory.SuccessTemplateMessage,
                                                                                                        resourcesFactory.FailureTemplateMessage));
            //Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("Invalid URI: The format of the URI could not be determined."));

        }
        /// <summary>
        ///     Test for the method that validates the valid blob Uri
        /// </summary>
        [Test]
        public void UsingManagedIdentity_WithValidUriAndTokenCredentials_Succeeds()
        {
            //Arrange
            var tokenCredential = new Mock<TokenCredential>();
            
            //Act
            var result = new AzureStorageLogProviderOptions(new Uri(_validUrl),
                                                            tokenCredential.Object,
                                                            true,
                                                            resourcesFactory.SuccessTemplateMessage,
                                                            resourcesFactory.FailureTemplateMessage);
            //Assert
            Assert.IsNotNull(result.AzureStorageBlobContainerUri);
            Assert.AreEqual(result.AzureStorageBlobContainerUri.AbsoluteUri, _validUrl);

        }

        [Test]
        public void UsingManagedIdentity_WhenTokenCredentialsAreNull_ThrowsException_()
        {
            TokenCredential tokenCredentials = null;
            //Act
            var exception = Assert.Throws<NullReferenceException>(() => new AzureStorageLogProviderOptions(new Uri(_validUrl),
                                                                tokenCredentials,
                                                                true,
                                                                resourcesFactory.SuccessTemplateMessage,
                                                                resourcesFactory.FailureTemplateMessage));
            //Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("The credential cannot be null when Azure storage option is set to enabled"));
        }

        [Test]
        public void UsingManagedIdentity_WhenStorageOptionIsNotSet_NoValidationHappens()
        {
            TokenCredential tokenCredentials = null;
            //Act
            //Assert
            Assert.DoesNotThrow(() => new AzureStorageLogProviderOptions(new Uri(_validUrl),
                                                                tokenCredentials,
                                                                false,
                                                                resourcesFactory.SuccessTemplateMessage,
                                                                resourcesFactory.FailureTemplateMessage));         
        }
    }
}