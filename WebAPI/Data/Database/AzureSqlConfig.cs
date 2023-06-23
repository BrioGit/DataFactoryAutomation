namespace WebAPI.Data.Database
{
    public class AzureSqlConfig
    {
        public int AzureSqlConfigId { get; set; }

        public string ServerDomainName { get; set; }

        public string DatabaseName { get; set; }

        public string SqlUser { get; set; }

        public string SqlPassword { get; set; }
    }
}
