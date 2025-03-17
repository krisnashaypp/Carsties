using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ServiceDefaults;

public enum ServiceType
{
    AUCTION,
    SEARCH,
    IDENTITY,
    BIDS,
    NOTIFICATION,
    FRONTEND
}

public static class CustomExtensions
{
    public static string GetServiceLocation(
        this WebApplicationBuilder builder, 
        ServiceType serviceType, 
        string alternateConfigKey = "",
        string protocol = "http")
    {
        var serviceName = serviceType.ToString().ToLower() + "service";
        var configKey = $"services:{serviceName}:{protocol}:0";
        
        var serviceLocation = builder.Configuration[configKey];

        if (!string.IsNullOrEmpty(serviceLocation)) return serviceLocation;
        
        if (!string.IsNullOrEmpty(alternateConfigKey))
        {
            var alternateServiceLocation = builder.Configuration[alternateConfigKey];
            if (!string.IsNullOrEmpty(alternateServiceLocation)) return alternateServiceLocation;
        }
            
        throw new InvalidOperationException($"Service location not found for {serviceType}. Configuration key: {configKey}. Alternate config key: {alternateConfigKey}");

    }
    
    public static string GetServiceLocation(
        this IConfiguration config, 
        ServiceType serviceType, 
        string alternateConfigKey = "",
        string protocol = "http")
    {
        var serviceName = serviceType.ToString().ToLower() + "service";
        var configKey = $"services:{serviceName}:{protocol}:0";
        
        var serviceLocation = config[configKey];

        if (!string.IsNullOrEmpty(serviceLocation)) return serviceLocation;
        
        if (!string.IsNullOrEmpty(alternateConfigKey))
        {
            var alternateServiceLocation = config[alternateConfigKey];
            if (!string.IsNullOrEmpty(alternateServiceLocation)) return alternateServiceLocation;
        }
            
        throw new InvalidOperationException($"Service location not found for {serviceType}. Configuration key: {configKey}. Alternate config key: {alternateConfigKey}");

    }
}