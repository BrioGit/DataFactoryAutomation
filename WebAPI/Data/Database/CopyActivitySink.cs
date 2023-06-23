namespace WebAPI.Data.Database
{
    public class CopyActivitySink
    {
        public int CopyActivitySinkId { get; set; }

        public virtual Dataset Dataset { get; set; }

        public string? SchemaName { get; set; }

        public string ObjectName { get; set; }

        public string WriteBehavior { get; set; }

        public string? ExternalIdFieldName { get; set; }
    }
}
