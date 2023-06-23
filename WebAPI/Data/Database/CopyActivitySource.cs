using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Database
{
    public class CopyActivitySource
    {
        [Key]
        public int CopyActivitySourceId { get; set; }

        public virtual Dataset Dataset { get; set; }

        public string? SchemaName { get; set; }

        public string ObjectName { get; set; }

        public string? Query { get; set; }
    }
}
    