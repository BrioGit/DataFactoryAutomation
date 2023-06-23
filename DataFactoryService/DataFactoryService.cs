using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.Rest.Serialization;

namespace DataFactoryAutomation;

public class DataFactoryService : IDataFactoryService
{
    public DataFactoryService(DataFactoryManagementClient client, string resourceGroup, string region)
    {
        _adfClient = client;
        _resourceGroup = resourceGroup;
        _region = region;
    }

    private DataFactoryManagementClient _adfClient { get; }
    private string _resourceGroup { get; }
    private string _region { get; }

    public void CreateDataFactory(string name)
    {
        Console.WriteLine("Creating data factory " + name + "...");
        var dataFactory = new Factory
        {
            Location = _region,
            Identity = new FactoryIdentity()
        };
        _adfClient.Factories.CreateOrUpdate(_resourceGroup, name, dataFactory);

        while (_adfClient.Factories.Get(_resourceGroup, name).ProvisioningState ==
               "PendingCreation")
            Thread.Sleep(1000);
        Console.WriteLine(SafeJsonConvert.SerializeObject(dataFactory, _adfClient.SerializationSettings));
        Console.WriteLine();
    }

    public void CreateSalesforceLinkedService(string adfName, string serviceName, string url, string username,
        string password, string securityToken, string apiVersion)
    {
        var sfLinkedService = new LinkedServiceResource(
            new SalesforceLinkedService
            {
                EnvironmentUrl = url,
                Username = username,
                Password = new SecureString(password),
                SecurityToken = new SecureString(securityToken),
                ApiVersion = apiVersion
            });
        _adfClient.LinkedServices.CreateOrUpdate(_resourceGroup, adfName, serviceName, sfLinkedService);
    }

    public void CreateAzureSqlLinkedService(string adfName, string serviceName, string serverDomain,
        string databaseName, string username, string password)
    {
        var sqlLinkedService = new LinkedServiceResource(
            new AzureSqlDatabaseLinkedService(
                $"Data Source={serverDomain};Initial Catalog={databaseName};User Id={username};Password={password};Integrated Security=False;Encrypt=True;Connect Timeout=30")
        );
        _adfClient.LinkedServices.CreateOrUpdate(_resourceGroup, adfName, serviceName, sqlLinkedService);
    }

    public async Task CreateSalesforceDataset(string adfName, string serviceName, string datasetName)
    {
        var salesforceDataset = new DatasetResource(
            new SalesforceObjectDataset
            {
                LinkedServiceName = new LinkedServiceReference
                {
                    ReferenceName = serviceName
                },
                Parameters = new Dictionary<string, ParameterSpecification>
                {
                    {"SFObject", new ParameterSpecification {Type = ParameterType.String}}
                },
                ObjectApiName = "@dataset().SFObject",
                Schema = "[]"
            });
        await _adfClient.Datasets.CreateOrUpdateAsync(_resourceGroup, adfName, $"{datasetName}", salesforceDataset);
    }

    public async Task CreateAzureSqlDataset(string adfName, string serviceName, string datasetName)
    {
        var sqlDataset = new DatasetResource(
            new AzureSqlTableDataset
            {
                LinkedServiceName = new LinkedServiceReference
                {
                    ReferenceName = serviceName
                },
                Parameters = new Dictionary<string, ParameterSpecification>
                {
                    {"SchemaName", new ParameterSpecification {Type = ParameterType.String}},
                    {"TableName", new ParameterSpecification {Type = ParameterType.String}}
                },
                AzureSqlTableDatasetSchema = "@dataset().SchemaName",
                Schema = "@dataset().SchemaName",
                Table = "@dataset().TableName"
            });
        await _adfClient.Datasets.CreateOrUpdateAsync(_resourceGroup, adfName, $"{datasetName}", sqlDataset);
    }

    public async Task CreatePipeline(string adfName, string pipelineName, PipelineResource pipeline)
    {
        await _adfClient.Pipelines.CreateOrUpdateAsync(_resourceGroup, adfName, pipelineName, pipeline);
    }

    /// <summary>
    ///     Executes the specified pipeline
    /// </summary>
    /// <param name="adfName">Data Factory name</param>
    /// <param name="pipelineName">Pipeline name</param>
    /// <returns>Azure execution id for the pipeline</returns>
    public async Task<string> TriggerPipeline(string adfName, string pipelineName)
    {
        var result = await _adfClient.Pipelines.CreateRunWithHttpMessagesAsync(_resourceGroup, adfName, pipelineName);
        var runResponse = result.Body;
        return runResponse.RunId;
    }

    /// <summary>
    ///     Gets the status of the pipeline
    /// </summary>
    /// <param name="adfName">Data Factory name</param>
    /// <param name="runId">Azure identifier for the pipeline run</param>
    /// <returns></returns>
    public async Task<string> GetPipelineStatus(string adfName, string runId)
    {
        var result = await _adfClient.PipelineRuns.GetAsync(_resourceGroup, adfName, runId);
        return result.Status;
    }
}