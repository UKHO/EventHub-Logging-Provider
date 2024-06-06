# Event Hub Log Provider

The Event Hub Log Provider provides a logging sink for the Microsoft.Extensions.Logging.Abstractions. Logs are sent to Event Hub as a JSON message. The EventHubLogProvider provides a number of standard properties to enrich every log message, and provides a mechanism to add application specific custom properties to logs.

Event hub log provider now supports authentication using both Managed Service Identity or connection string/SAS Uri to Event hub or Azure storage container respectively. However they both should use either Managed Identity to both Eventhub and Azure storage or neither. 

Configuring using managed identity or connection string is shown below.

## Getting Started

### Installation Guide

This package is available from NuGet: UKHO.Logging.EventHubLogProvider

```bash
    nuget install UKHO.Logging.EventHubLogProvider
```

There are two recommended setups depending on the version of .NET: a legacy setup for .NET Core, and a setup for .NET 5/6+.

#### .NET 5/6+ Setup

The EventHubLogProvider is added to the ```IServiceCollection``` service collection via an ```ILoggingBuilder```.  

NuGet packages can be installed for extensions to the builder, for example adding console logging in the below statement ```loggingBuilder.AddConsole();``` would require installing the package ```ConsoleLoggerExtensions```

#### Configuration using connection string for Event hub

```cs
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
        loggingBuilder.AddAzureWebAppDiagnostics();
        var eventHubLoggingConfiguration = new EventHubLoggingConfiguration();
        configuration.GetSection(EventHubLoggingConfiguration.ConfigSection).Bind(eventHubLoggingConfiguration);
        if (!string.IsNullOrEmpty(eventHubLoggingConfiguration.ConnectionString))
        {
            loggingBuilder.AddEventHub(options =>
            {
                options.Environment = eventHubLoggingConfiguration.Environment;
                options.DefaultMinimumLogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), eventHubLoggingConfiguration.MinimumLoggingLevel, true);
                options.MinimumLogLevels["UKHO"] = (LogLevel)Enum.Parse(typeof(LogLevel), eventHubLoggingConfiguration.UkhoMinimumLoggingLevel, true);
                options.EventHubConnectionString = eventHubLoggingConfiguration.ConnectionString;
                options.EventHubEntityPath = eventHubLoggingConfiguration.EntityPath;
                options.System = eventHubLoggingConfiguration.System;
                options.Service = eventHubLoggingConfiguration.Service;
                options.NodeName = eventHubLoggingConfiguration.NodeName;
                options.AdditionalValuesProvider = ConfigAdditionalValuesProvider;
            });
        }
    });
```

#### Configuration using Managed identity to Event hub

```cs
var eventHubLogProviderOptions = builder.Configuration.GetSection("EventHubLogProviderOptions").Get<EventHubLogProviderOptions>();
ArgumentNullException.ThrowIfNull(eventHubLogProviderOptions);

builder.Logging.AddEventHub(options =>
{         
    options.EventHubFullyQualifiedNamespace = eventHubLogProviderOptions.EventHubFullyQualifiedNamespace,
    options.TokenCredential = new DefaultAzureCredential(), 
    options.EventHubEntityPath = eventHubLogProviderOptions.EntityPath;
    options.EnableConnectionValidation = eventHubLogProviderOptions.EnableConnectionValidation;
    options.DefaultMinimumLogLevel = eventHubLogProviderOptions.MinimumLoggingLevel;
    options.MinimumLogLevels["UKHO"] = eventHubLogProviderOptions.UkhoMinimumLoggingLevel;

    options.Environment = eventHubLogProviderOptions.Environment;
    options.System = eventHubLogProviderOptions.System;
    options.Service = eventHubLogProviderOptions.Service;
    options.NodeName = eventHubLogProviderOptions.NodeName;
    options.AdditionalValuesProvider = ConfigAdditionalValuesProvider;
});
```

Please note that following 2 options are added for authenticating with Managed Identity

- EventHubFullyQualifiedNamespace - The fully qualified Event Hubs namespace to connect to. This is likely to be similar to {yournamespace}.servicebus.windows.net.
- TokenCredential - The Azure managed identity credential to use for authorization.  

[!NOTE] The application (or user if running in Visual Studio) will require `Azure Event Hubs Data Sender` role on the Event hub. 

If you've upgraded from an earlier version of .NET Core and have not migrated to the new minimal hosting model, i.e., there is still a startup.cs, the above code should be added to the ```ConfigureServices```:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(loggingBuilder =>
    {
            ...
}

```

An ```HttpContextAccessor``` is unavailable in the ```ConfigureServices``` method, however, a reference to it can be acquired in the ```Configure``` method:

```cs
public class Startup
{
    private readonly IConfiguration configuration;
    private IHttpContextAccessor _httpContextAccessor;
...


public void Configure(IApplicationBuilder app,
                            IWebHostEnvironment env
                            IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    ...
```

Then, the additional values can be gathered as follows:

```cs
private void ConfigAdditionalValuesProvider(IDictionary<string, object> additionalValues)
{
    if (_httpContextAccessor.HttpContext != null)
    {
        additionalValues["_RemoteIPAddress"] =
            _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();

        additionalValues["_User-Agent"] =
            _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;

        additionalValues["_AssemblyVersion"] = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<AssemblyFileVersionAttribute>().Single()
            .Version;

        additionalValues["_X-Correlation-ID"] =
            _httpContextAccessor.HttpContext.Request.Headers?[""].FirstOrDefault() ?? string.Empty;
    }
}
```

#### Legacy .NET Core Setup

The EventHubLogProvider is added to a LoggerFactory as follows:

```cs
var connectionString = ""; //Connection string to Event Hub with write permissions.
var entityPath = ""; //Event Hub entity path.
loggerFactory.AddEventHub(
    config =>
    {
        /*
        optional(AzureStorageLogProviderOptions)
        Setting this property and setting ("AzureStorageOptions:Enabled") = true 
        enables the azure storage logging provider 
        (for messages with size >= 1Mb)  
    
        Please check "Azure Storage Logging Provider" for more information.
        */
        config.AzureStorageLogProviderOptions = new AzureStorageLogProviderOptions(
                                        Configuration.GetValue<String>("AzureStorageOptions:SasUrl")
                                        ,Configuration.GetValue<Boolean>("AzureStorageOptions:Enabled")
                                        ,Configuration.GetValue<String> ("AzureStorageOptions:SuccessfulMessageTemplate") 
                                        ,Configuration.GetValue<String>("AzureStorageOptions:FailedMessageTemplate")
                                        );
         
        
        config.Environment = "Production";
        config.DefaultMinimumLogLevel = LogLevel.Warning;
        config.MinimumLogLevels["Microsoft.AspNetCore"] = LogLevel.Trace;
        config.MinimumLogLevels["Microsoft.AspNetCore.Server"] = LogLevel.Error;
        config.EventHubConnectionString = connectionString;
        config.EventHubEntityPath = entityPath;
        config.System = "My System Name";
        config.Service = "My Service Name";
        config.NodeName = "Node 123";
        config.AdditionalValuesProvider = additionalValues =>
                                        {
                                            additionalValues["_AssemblyVersion"] = Assembly.GetExecutingAssembly()
                                                .GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;
                                            additionalValues["_X-Correlation-ID"] = correlationId;
                                        };
        config.ValidateConnectionString = true;
    });
```

Within a standard ASP .Net Core project, this provider is best added in the Start-up's `Configure` method as this will allow access to a `IHttpContextAccessor` for injecting request data to all logs.

```cs
 public void Configure(IApplicationBuilder app,
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory,
                              IHttpContextAccessor httpContextAccessor)
    {
        ...
   
        loggerFactory.AddConsole();
        loggerFactory.AddEventHub(
            config =>
            {
                /*
                optional(AzureStorageLogProviderOptions)
                Setting this property and setting ("AzureStorageOptions:Enabled") = true 
                enables the azure storage logging provider 
                (for messages with size >= 1Mb)  
    
                Please check "Azure Storage Logging Provider" for more information.
                */
                config.AzureStorageLogProviderOptions = new AzureStorageLogProviderOptions(
                                        Configuration.GetValue<String>("AzureStorageOptions:SasUrl")
                                        ,Configuration.GetValue<Boolean>("AzureStorageOptions:Enabled")
                                        ,Configuration.GetValue<String>                                 ("AzureStorageOptions:SuccessfulMessageTemplate") 
                                        ,Configuration.GetValue<String>("AzureStorageOptions:FailedMessageTemplate")
                                        );
                config.Environment = "Production";
                config.DefaultMinimumLogLevel = LogLevel.Warning;
                config.MinimumLogLevels["Microsoft.AspNetCore"] = LogLevel.Trace;
                config.MinimumLogLevels["Microsoft.AspNetCore.Server"] = LogLevel.Error;
                config.EventHubConnectionString = connectionString;
                config.EventHubEntityPath = entityPath;
                config.System = "My System Name";
                config.Service = "My Service Name";
                config.NodeName = "Node 123";
                config.AdditionalValuesProvider = additionalValues =>
                                                {
                                                    additionalValues["_AssemblyVersion"] = Assembly.GetExecutingAssembly()
                                                        .GetCustomAttributes<AssemblyFileVersionAttribute>().Single().Version;
                                                    additionalValues["_X-Correlation-ID"] = correlationId;
                                                    additionalValues["_RemoteIPAddress"] = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                                                    additionalValues["_User-Agent"] = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
                                                };
                config.ValidateConnectionString = true;
            });
    }
```

## Customisation of Log Parameter Serialization

The default log parameter serialization uses NewtonSoft.Net JsonConvert to serialize the log parameters to JSON. On occasion, it maybe desirable to customise the JSON produced to control how individual properties are serialized. This can be done by providing custom converters that extend the `Newtonsoft.Json.JsonConverter` class:

```cs
loggerFactory.AddEventHub(
            config =>
            {
                ...
                config.CustomLogSerializerConverters = new List<JsonConverter> { new VersionJsonConverter() };
                ...
            });
```

The JsonConverter must implement `WriteJson`, but the `ReadJson` method can be left unimplemented and the `CanRead` property can return false. The `CanConvert` method must only return true for the types that you wish to override the serialization of.  More details about custom converters can be found in the JsonConvert documentation <https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm>.

```cs
    public class VersionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Version version)
            {
                writer.WriteValue($"{version.MajorVersion}.{version.MinorVersion}.{version.Build}.{version.Revision}");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Version);
        }

        public override bool CanRead => false;
    }
```

## Azure Storage Logging Provider

The Azure Storage Logging Provider is a storage logging provider that stores messages with size equal or greater than 1Mb into an Azure storage container. Finally, it updates the log entry with the azure storage blob details. Enabling the Azure storage provider is optional.

The provider can be enabled by providing the AzureStorageLogProviderOptions.
It supports bothe managed identity or SAS url for authentication. Below are example for each.

### Configuration using Azure Storage conatiner SAS url

 The AzureStorageLogProviderOptions model consists of:

```cs
//The SAS url for the storage container, recommended rights set : racwl
string azureStorageContainerSasUrlString  
//Enables (true) or disables(false) the Azure storage logging provider
bool azureStorageLoggerEnabled,
//A template for the messages that are successfully stored*
string successfulMessageTemplate,
//A template for the messages that failed to be stored*
string failedMessageTemplate
/*
The templates are configurable. 
The parameters must be added with the following format: {{property_name}} 

Available properties:

<param name="reasonPhrase">The result reason phrase</param>
<param name="statusCode">The http status code</param>
<param name="requestId">The client request Id</param>
<param name="fileSHA">The blob SHA 256</param>
<param name="isStored">The flag that determines if the result was successful/failed</param>
<param name="blobFullName">The blob full name</param>
<param name="fileSize">The file size (optional)</param>
<param name="modifiedDate">The modified date(optional)</param>

public string ReasonPhrase { get; set; }
public int StatusCode { get; set; }
public string RequestId { get; set; }
public string FileSHA { get; set; }
public bool IsStored { get; set; }
public string BlobFullName { get; set; }
public long FileSize { get; set; }
public DateTime ModifiedDate { get; set; }

*/
```

#### Example of a configuration section(json) 

```json
    "AzureStorageOptions": {
    "SasUrl": "the_sas_url", //removed for security reasons
    "Enabled": true,
    "SuccessfulMessageTemplate": "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}",
    "FailedMessageTemplate": "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}"
  }
```

### Configuration using Managed identity

 The AzureStorageLogProviderOptions model consists of:

 ```
options.AzureStorageLogProviderOptions = new AzureStorageLogProviderOptions(
                        // Azure storage blob container uri
                        new Uri(builder.Configuration.GetValue<String>("AzureStorageOptions:BlobContainerUri"),
                        // The Azure managed identity credential to use for authorization.
                        new DefaultAzureCredential(),
                        //Enables (true) or disables(false) the Azure storage logging provider
                        builder.Configuration.GetValue<String>("AzureStorageOptions:AzureStorageLoggerEnabled"),
                        //A template for the messages that are successfully stored
                        builder.Configuration.GetValue<String>("AzureStorageOptions:SuccessMessageTemaplate"),
                        //A template for the messages that failed to be stored*                      
                        builder.Configuration.GetValue<String>("AzureStorageOptions:FailedMessageTemplate")
                        );


```

### Example of a configuration section(json) 

```json
    "AzureStorageOptions": {
    "BlobContainerUri": "blob container uri", //This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}"
    "Enabled": true,
    "SuccessfulMessageTemplate": "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}",
    "FailedMessageTemplate": "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}"
  }
```

[!NOTE] The application (or user if running in Visual Studio) will require `Storage Blob Data Contributor` role on the Event hub. 

### Azure Storage Provider Functional Diagram

[The diagram (Visio) can be downloaded using the following link](/docs/Technical%20Documentation/Logging%20Provider%20-%20Azure%20Storage%20Logging.vsdx)

## EventHub Logging Provider Help File

[Version 1.0.0](/docs/Technical%20Documentation/Help%20Files/1.0.0/Documentation.chm)

## How to Engage, Contribute, and Give Feedback

Some of the best ways to contribute are to try things out, file issues and make pull-requests.

---

The UK Hydrographic Office (UKHO) supplies hydrographic information to protect lives at sea. Maintaining the confidentially, integrity and availability of our services is paramount. Found a security bug? Please report it to us at UKHO-ITSO@gov.co.uk
