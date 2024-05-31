using System;

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
        private const string _invalidUrl = "-test";
        private const string _validUrl = "https://test.com/";

        /// <summary>
        ///     Test for the method that validates the url string (When the url string is an invalid url string)
        /// </summary>
        [Test]
        public void Test_ValidateSasUrl_InvalidUrl()
        {
            var url = _invalidUrl;

            //Act
            var exception = Assert.Throws<UriFormatException>(() => new AzureStorageLogProviderOptions(url,
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
            var exception = Assert.Throws<NullReferenceException>(() => new AzureStorageLogProviderOptions(url,
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
            var url = _validUrl;

            var result = new AzureStorageLogProviderOptions(url, true, resourcesFactory.SuccessTemplateMessage, resourcesFactory.FailureTemplateMessage);

            Assert.IsNotNull(result.AzureStorageContainerSasUrl);
            Assert.AreEqual(result.AzureStorageContainerSasUrl.AbsoluteUri, url);
        }
        
        /*
         * Need to implement new tests that cover
         * 1. The new constructor
         * 2. The exceptions thrown
         *
         * Use Stryker and check the surviving mutants for the AzureStorageLogProviderOptions
         */
    }
}