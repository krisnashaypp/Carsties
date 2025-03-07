using Microsoft.AspNetCore.Builder;

namespace ServiceDefaults;

public enum ServiceType
{
    AUCTION,
    SEARCH,
    IDENTITY
}

public static class CustomExtensions
{
    public static string GetServiceLocation(
        this WebApplicationBuilder builder, 
        ServiceType serviceType, 
        string alternateConfigKey = "")
    {
        var serviceName = serviceType.ToString().ToLower() + "service";
        var configKey = $"services:{serviceName}:http:0";
        
        var serviceLocation = builder.Configuration[configKey];

        if (!string.IsNullOrEmpty(serviceLocation)) return serviceLocation;
        
        if (!string.IsNullOrEmpty(alternateConfigKey))
        {
            var alternateServiceLocation = builder.Configuration[alternateConfigKey];
            if (!string.IsNullOrEmpty(alternateServiceLocation)) return alternateServiceLocation;
        }
            
        throw new InvalidOperationException($"Service location not found for {serviceType}. Configuration key: {configKey}. Alternate config key: {alternateConfigKey}");

    }
}