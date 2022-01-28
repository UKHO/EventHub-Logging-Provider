# Event Hub Log Provider

The Event Hub Log Provider provides a logging sink for the Microsoft.Extensions.Logging.Abstractions. Logs are sent to Event Hub as a JSON message. The EventHubLogProvider provides a number of standard properties to enrich every log message, and provides a mechanism to add application specific custom properties to logs.

## Getting Started

### Installation Guide

This package is available from NuGet: UKHO.Logging.EventHubLogProvider

```bash
    nuget install UKHO.Logging.EventHubLogProvider
```

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

## Azure Storage Logging Provider

The Azure Storage Logging Provider is a storage logging provider that stores messages with size equal or greater than 1Mb into an Azure storage container. Finally, it updates the log entry with the azure storage blob details. Enabling the Azure storage provider is optional.

The provider can be enabled by providing the AzureStorageLogProviderOptions model which consists of:

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

### Example of a configuration section(json)

```json
    "AzureStorageOptions": {
    "SasUrl": "the_sas_url", //removed for security reasons
    "Enabled": true,
    "SuccessfulMessageTemplate": "Azure Storage Logging: A blob with the error details was created at {{BlobFullName}}. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}} Sha256: {{FileSHA}} FileSize(Bs): {{FileSize}} FileModifiedDate: {{ModifiedDate}}",
    "FailedMessageTemplate": "Azure Storage Logging: Storing blob failed. Reason: ErrorMessageEqualOrGreaterTo1MB ResponseMessage: {{ReasonPhrase}} ResponseCode: {{StatusCode}} RequestId: {{RequestId}}"
  }
```

### Azure Storage Provider Functional Diagram

[The diagram (Visio) can be downloaded using the following link](/docs/Technical%20Documentation/Logging%20Provider%20-%20Azure%20Storage%20Logging.vsdx)

## EventHub Logging Provider Help File

[Version 1.0.0](/docs/Technical%20Documentation/Help%20Files/1.0.0/Documentation.chm)

## How to Engage, Contribute, and Give Feedback

Some of the best ways to contribute are to try things out, file issues and make pull-requests.

---

The UK Hydrographic Office (UKHO) supplies hydrographic information to protect lives at sea. Maintaining the confidentially, integrity and availability of our services is paramount. Found a security bug? Please report it to us at UKHO-ITSO@gov.co.uk
