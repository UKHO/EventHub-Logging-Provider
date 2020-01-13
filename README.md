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

## How to Engage, Contribute, and Give Feedback

Some of the best ways to contribute are to try things out, file issues and make pull-requests.

---

The UK Hydrographic Office (UKHO) supplies hydrographic information to protect lives at sea. Maintaining the confidentially, integrity and availability of our services is paramount. Found a security bug? Please report it to us at UKHO-ITSO@gov.co.uk