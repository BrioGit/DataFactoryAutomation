using Microsoft.Azure.Management.DataFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFactoryAutomation
{
    public static class DataFactoryServiceConfiguration
    {
        public static void AddDataFactoryService(this IServiceCollection services, AzureConfiguration config)
        {
            var adfClient = GetDataFactoryClient(config);
            services.AddScoped<IDataFactoryService>(client => new DataFactoryService(adfClient, config.ResourceGroup, config.Region));
        }

        internal static DataFactoryManagementClient GetDataFactoryClient(AzureConfiguration config)
        {
            var app = ConfidentialClientApplicationBuilder.Create(config.ApplicationId)
             .WithAuthority("https://login.microsoftonline.com/" + config.TenantId)
             .WithClientSecret(config.ClientSecret)
             .WithLegacyCacheCompatibility(false)
             .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
             .Build();

            var result = app.AcquireTokenForClient(
              new string[] { "https://management.azure.com//.default" })
               .ExecuteAsync().Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            return new DataFactoryManagementClient(cred)
            {
                SubscriptionId = config.SubscriptionId
            };
        }
    }
}
