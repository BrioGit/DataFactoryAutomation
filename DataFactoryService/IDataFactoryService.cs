using Microsoft.Azure.Management.DataFactory.Models;

namespace DataFactoryAutomation
{
    public interface IDataFactoryService
    {
        /// <summary>
        ///     Create a new data factory
        /// </summary>
        void CreateDataFactory(string name);

        /// <summary>
        ///     Create a new Salesforce linked service
        /// </summary>
        void CreateSalesforceLinkedService(string adfName, string serviceName, string url, string username, string password, string securityToken, string apiVersion);

        /// <summary>
        ///     Create a new Azure SQL database linked service
        /// </summary>
        void CreateAzureSqlLinkedService(string adfName, string serviceName, string serverDomain, string databaseName, string username, string password);

        /// <summary>
        ///     Create a new Salesforce dataset
        /// </summary>
        Task CreateSalesforceDataset(string adfName, string serviceName, string datasetName);

        /// <summary>
        ///     Create a new Azure SQL database dataset
        /// </summary>
        Task CreateAzureSqlDataset(string adfName, string serviceName, string datasetName);

        /// <summary>
        ///     Create a new Pipeline and associated activities
        /// </summary>
        Task CreatePipeline(string adfName, string pipelineName, PipelineResource pipeline);

        /// <summary>
        ///     Trigger a Pipeline run and return the run id
        /// </summary>
        Task<string> TriggerPipeline(string adfName, string pipelineName);

        /// <summary>
        ///     Get Pipeline run status
        /// </summary>
        Task<string> GetPipelineStatus(string adfName, string runId);
    }
}
