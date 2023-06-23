using WebAPI.Data.Context;

namespace WebAPI.Data.Database;

public static class DbInitializer
{
    /// <summary>
    ///     Adds the demo Salesforce and Azure Database connectors and linked
    ///     services to the database
    /// </summary>
    /// <param name="context"></param>
    /// <param name="salesforceConfig"></param>
    /// <param name="azureSqlConfig"></param>
    public static void Initialize(ConfigContext context,
        SalesforceConfig salesforceConfig,
        AzureSqlConfig azureSqlConfig)
    {
        // If the DataFactories table has any data in it then assume the
        // database has already been initialized
        if (context.DataFactories.Any()) return;

        var dataFactory = new DataFactory {FactoryName = "ADF-Automation-Sample"};

        // Linked services
        var sfConfig = new SalesforceConfig
        {
            EnvironmentUrl = salesforceConfig.EnvironmentUrl,
            Username = salesforceConfig.Username,
            Password = salesforceConfig.Password,
            SecurityToken = salesforceConfig.SecurityToken,
            ApiVersion = salesforceConfig.ApiVersion
        };

        var sfLinkService = new LinkedService
        {
            ServiceName = "SalesforceLinkedService",
            ServiceType = LinkedServiceType.Salesforce,
            SalesforceConfig = sfConfig
        };

        var sqlLinkService = new LinkedService
        {
            ServiceName = "AzureSqlLinkedService",
            ServiceType = LinkedServiceType.AzureSql,
            AzureSqlConfig = azureSqlConfig
        };

        dataFactory.LinkedServices = new List<LinkedService> {sfLinkService, sqlLinkService};

        // Datasets
        var sfDataset = new Dataset
        {
            Name = "SalesforceDataset",
            ServiceType = LinkedServiceType.Salesforce,
            SalesforceLinkedService = sfLinkService
        };

        var sqlDataset = new Dataset
        {
            Name = "SqlDataset",
            ServiceType = LinkedServiceType.AzureSql,
            AzureSqlLinkedService = sqlLinkService
        };

        dataFactory.Datasets = new List<Dataset> {sfDataset, sqlDataset};

        // CopyActivities
        var copyActivitySfToSql = new CopyActivity
        {
            Name = "Pull Account",
            Source = new CopyActivitySource
            {
                Dataset = sfDataset,
                ObjectName = "Account",
                Query =
                    "select ID, Type, BillingStreet, BillingCity, BillingState, BillingPostalCode, BillingCountry from Account"
            },
            Sink = new CopyActivitySink
            {
                Dataset = sqlDataset,
                SchemaName = "ADF",
                ObjectName = "SF_Account",
                WriteBehavior = "upsert",
                ExternalIdFieldName = "Id"
            }
        };

        var pipelineSfToSql = new Pipeline
        {
            Name = "Pull Salesforce Source",
            CopyActivities = new List<CopyActivity> {copyActivitySfToSql}
        };

        var copyActivitySqlToSf = new CopyActivity
        {
            Name = "Update Account",
            Source = new CopyActivitySource
            {
                Dataset = sqlDataset,
                ObjectName = "SF_Account",
                SchemaName = "ADF"
            },
            Sink = new CopyActivitySink
            {
                Dataset = sfDataset,
                ObjectName = "Account",
                WriteBehavior = "upsert",
                ExternalIdFieldName = "Id"
            }
        };

        var pipelineSqlToSf = new Pipeline
        {
            Name = "Push Salesforce Updates",
            CopyActivities = new List<CopyActivity> {copyActivitySqlToSf}
        };

        dataFactory.Pipelines = new List<Pipeline> {pipelineSfToSql, pipelineSqlToSf};

        context.Add(dataFactory);
        context.SaveChanges();
    }
}