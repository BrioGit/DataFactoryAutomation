using DataFactoryAutomation;
using Microsoft.Azure.Management.DataFactory.Models;
using WebAPI.Data.Database;
using WebAPI.Data.Services;
using CopyActivity = Microsoft.Azure.Management.DataFactory.Models.CopyActivity;

namespace WebAPI.Business;

public class DataFactoryAutomation
{
    private readonly IDataFactoryService _adfService;
    private readonly ConfigService _configService;

    public DataFactoryAutomation(IDataFactoryService adfService, ConfigService configService)
    {
        _adfService = adfService;
        _configService = configService;
    }

    /// <summary>
    ///     Executes the processes required to create a new DataFactory based on the configuration
    /// </summary>
    /// <param name="dataFactoryId">Database identifier of the DataFactory instance to create</param>
    /// <returns><code>true</code> if the process to create the DataFactory was successful</returns>
    public async Task<bool> CreateDataFactory(int dataFactoryId)
    {
        var dfConfig = await _configService.GetDataFactoryByIdAsync(dataFactoryId);
        if (dfConfig == null) return false;

        // create datafactory
        _adfService.CreateDataFactory(dfConfig.FactoryName);

        // create linked services
        foreach (var linkedService in dfConfig.LinkedServices)
            switch (linkedService.ServiceType)
            {
                case LinkedServiceType.Salesforce:
                    var sfConfig = linkedService.SalesforceConfig;
                    _adfService.CreateSalesforceLinkedService(dfConfig.FactoryName, linkedService.ServiceName,
                        sfConfig.EnvironmentUrl, sfConfig.Username, sfConfig.Password, sfConfig.SecurityToken,
                        sfConfig.ApiVersion);
                    break;

                case LinkedServiceType.AzureSql:
                    var sqlConfig = linkedService.AzureSqlConfig;
                    _adfService.CreateAzureSqlLinkedService(dfConfig.FactoryName, linkedService.ServiceName,
                        sqlConfig.ServerDomainName, sqlConfig.DatabaseName, sqlConfig.SqlUser, sqlConfig.SqlPassword);
                    break;
            }

        // create datasets
        foreach (var dataset in dfConfig.Datasets)
            switch (dataset.ServiceType)
            {
                case LinkedServiceType.Salesforce:
                    await _adfService.CreateSalesforceDataset(dfConfig.FactoryName,
                        dataset.SalesforceLinkedService.ServiceName, dataset.Name);
                    break;

                case LinkedServiceType.AzureSql:
                    await _adfService.CreateAzureSqlDataset(dfConfig.FactoryName,
                        dataset.AzureSqlLinkedService.ServiceName, dataset.Name);
                    break;
            }

        // create pipelines
        foreach (var pipeline in dfConfig.Pipelines)
        {
            var activties = new List<Activity>();

            // create copy activities for this pipeline
            foreach (var copyActivity in pipeline.CopyActivities)
                if (copyActivity.Source.Dataset.ServiceType == LinkedServiceType.Salesforce &&
                    copyActivity.Sink.Dataset.ServiceType == LinkedServiceType.AzureSql)
                {
                    var activity = CreateSalesforceToSqlCopyActivity(copyActivity);
                    activties.Add(activity);
                }
                else if (copyActivity.Source.Dataset.ServiceType == LinkedServiceType.AzureSql &&
                         copyActivity.Sink.Dataset.ServiceType == LinkedServiceType.Salesforce)
                {
                    var activity = CreateSqlToSalesforceCopyActivity(copyActivity);
                    activties.Add(activity);
                }

            // additional activity types could be generated here

            // create pipeline
            var pipelineResource = new PipelineResource
            {
                Activities = activties
            };
            await _adfService.CreatePipeline(dfConfig.FactoryName, pipeline.Name, pipelineResource);
        }

        return true;
    }

    internal CopyActivity CreateSalesforceToSqlCopyActivity(Data.Database.CopyActivity copyActivity)
    {
        var upsertSettings = new SqlUpsertSettings();
        if (copyActivity.Sink.WriteBehavior == "upsert")
        {
            upsertSettings.UseTempDB = false;
            upsertSettings.InterimSchemaName = "";
            upsertSettings.Keys = new[] {copyActivity.Sink.ExternalIdFieldName};
        }

        var activity = new CopyActivity
        {
            Name = copyActivity.Name,
            Inputs = new List<DatasetReference>
            {
                new()
                {
                    ReferenceName = copyActivity.Source.Dataset.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {"SFObject", copyActivity.Source.ObjectName}
                    }
                }
            },
            Outputs = new List<DatasetReference>
            {
                new()
                {
                    ReferenceName = copyActivity.Sink.Dataset.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {"schemaName", copyActivity.Sink.SchemaName},
                        {"tableName", copyActivity.Sink.ObjectName}
                    }
                }
            },
            Source = new SalesforceSource
            {
                Query = copyActivity.Source.Query
            },
            Sink = new SqlSink
            {
                WriteBehavior = copyActivity.Sink.WriteBehavior,
                UpsertSettings = upsertSettings,
                TableOption = "autoCreate"
            },
            EnableSkipIncompatibleRow = true
        };
        return activity;
    }

    internal CopyActivity CreateSqlToSalesforceCopyActivity(Data.Database.CopyActivity copyActivity)
    {
        var source = new AzureSqlSource();
        if (!string.IsNullOrEmpty(copyActivity.Source.Query)) source.SqlReaderQuery = copyActivity.Source.Query;

        var activity = new CopyActivity
        {
            Name = copyActivity.Name,
            Inputs = new List<DatasetReference>
            {
                new()
                {
                    ReferenceName = copyActivity.Source.Dataset.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {"schemaName", copyActivity.Source.SchemaName ?? "dbo"},
                        {"tableName", copyActivity.Source.ObjectName}
                    }
                }
            },
            Outputs = new List<DatasetReference>
            {
                new()
                {
                    ReferenceName = copyActivity.Sink.Dataset.Name,
                    Parameters = new Dictionary<string, object>
                    {
                        {"SFObject", copyActivity.Sink.ObjectName}
                    }
                }
            },
            Source = source,
            Sink = new SalesforceSink
            {
                WriteBehavior = copyActivity.Sink.WriteBehavior,
                ExternalIdFieldName = copyActivity.Sink.ExternalIdFieldName,
                IgnoreNullValues = true
            },
            EnableSkipIncompatibleRow = true
        };
        return activity;
    }

    /// <summary>
    ///     Executes the processes required to trigger a new Pipeline run
    /// </summary>
    /// <param name="factoryId">Database identifier of the associated Data Factory</param>
    /// <param name="pipelineId">Database identifier of the Pipeline instance to run</param>
    /// <returns><code>runId</code> identifier for the pipeline instance</returns>
    public async Task<string> TriggerPipeline(int factoryId, int pipelineId)
    {
        var dfConfig = await _configService.GetDataFactoryByIdAsync(factoryId);
        if (dfConfig == null) return string.Empty;
        var pipeline = dfConfig.Pipelines.FirstOrDefault(x => x.PipelineId == pipelineId);
        if (pipeline == null) return string.Empty;

        return await _adfService.TriggerPipeline(dfConfig.FactoryName, pipeline.Name);
    }

    /// <summary>
    ///     Executes the processes required to check the status of a Pipeline run
    /// </summary>
    /// <param name="factoryId">Database identifier of the associated Data Factory</param>
    /// <param name="runId">Azure identifier of the Pipeline run</param>
    /// <returns><code>status</code> status of the run</returns>
    public async Task<string> GetPipelineStatus(int factoryId, string runId)
    {
        var dfConfig = await _configService.GetDataFactoryByIdAsync(factoryId);
        if (dfConfig == null) return string.Empty;
        return await _adfService.GetPipelineStatus(dfConfig.FactoryName, runId);
    }
}