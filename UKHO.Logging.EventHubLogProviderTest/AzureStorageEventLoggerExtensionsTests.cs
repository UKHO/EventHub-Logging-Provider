using System;
using System.Collections.Generic;
using System.Text;
using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Extensions;

using NUnit.Framework;

using UKHO.Logging.EventHubLogProvider.AzureStorageEventLogging.Models;


namespace UKHO.Logging.EventHubLogProviderTest
{
    [TestFixture]
    public class AzureStorageEventLoggerExtensionsTests
    {
        [Test]
        public void Test_IsLongMessage_WithMessageGreaterThanMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 1025);
            bool result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_IsLongMessage_WithMessageLessThanMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 512);
            bool result = message.IsLongMessage(size);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_IsLongMessage_WithMessageEqualToMbs()
        {
            int size = 1;
            string message = GenerateTestMessage(1024 * 1024);
            bool result = message.IsLongMessage(size);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = null;
            string message = GenerateTestMessage(1024 * 1024);
            bool result =  azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message,1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithBuilderModelNotNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(null);
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLogProviderOptionsNotNull()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledTrue()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithAzureStorageLoggerEnabledFalse()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", false));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageGreaterThanDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1025);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }
        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageEqualToDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 1024);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_NeedsAzureStorageLogging_WithMessageLessThanDefinedSize()
        {
            AzureStorageBlobContainerBuilder azureStorageBlobContainerBuilder = new AzureStorageBlobContainerBuilder(new AzureStorageLogProviderOptions("https://test.com", true));
            string message = GenerateTestMessage(1024 * 512);
            bool result = azureStorageBlobContainerBuilder.NeedsAzureStorageLogging(message, 1);
            Assert.IsFalse(result);
        }
        [Test]
        public void TestGetLogEntryPropertyValueKeyExists()
        {
            Dictionary<string, object> set = new Dictionary<string, object>();
            set.Add("test_key_exists","test_value");
            string result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNotNull(result);
            Assert.AreEqual("test_value", result);
        }
        [Test]
        public void TestGetLogEntryPropertyValueKeyNotExists()
        {
            Dictionary<string, object> set = new Dictionary<string, object>();
            set.Add("test_key__not_exists", "test_value");
            string result = set.GetLogEntryPropertyValue("test_key_exists");
            Assert.IsNull(result);
        }
        /// <summary>
        /// Generates a test string message
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns>A message</returns>
        private string GenerateTestMessage(int size)
        {
            var charsPool = "ABCDEFGHJKLMNOPQRSTVUWXYZ1234567890";
            var charsArray = new char[size];

            var rand = new Random();

            for (int c = 0; c < charsArray.Length; c++)
            {
                charsArray[c] = charsPool[rand.Next(charsPool.Length)];
            }

            return new String(charsArray);
        }
    }
}
