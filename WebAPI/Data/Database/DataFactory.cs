using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Database
{
    public class DataFactory
    {
        public int DataFactoryId { get; set; }

        public string FactoryName { get; set; }

        public IEnumerable<LinkedService> LinkedServices { get; set; }

        public IEnumerable<Dataset> Datasets { get; set; }

        public IEnumerable<SalesforceConfig> SalesforceLinkedServices { get; set;}

        public IEnumerable<AzureSqlConfig> AzureSqlLinkedServices { get; set;}

        public IEnumerable<Pipeline> Pipelines { get; set; }
    }
}
