namespace WebAPI.Data.Database
{
    public class Dataset
    {
        public int DatasetId { get; set; }
        public string Name { get; set; }
        public LinkedServiceType ServiceType { get; set; }
        public int? AzureSqlLinkedServiceId { get; set; }

        public virtual LinkedService AzureSqlLinkedService { get; set; }

        public int? SalesforceLinkedServiceId { get; set; }

        public virtual LinkedService SalesforceLinkedService { get; set; }
    }
}
