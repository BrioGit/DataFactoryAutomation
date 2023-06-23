using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Database
{
    public class LinkedService
    {
        public int LinkedServiceId { get; set; }
        
        public int DataFactoryId { get; set; }

        public string ServiceName { get; set; }

        public LinkedServiceType ServiceType { get; set; }

        public int? AzureSqlConfigId { get; set; }

        public virtual AzureSqlConfig AzureSqlConfig { get; set; }

        public int? SalesforceConfigId { get; set; }

        public virtual SalesforceConfig SalesforceConfig { get; set; }

    }

    public enum LinkedServiceType
    {
        AzureSql = 0,
        Salesforce = 1
    }
}
