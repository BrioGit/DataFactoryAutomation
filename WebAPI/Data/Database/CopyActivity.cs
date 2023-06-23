using System.ComponentModel.DataAnnotations;

namespace WebAPI.Data.Database
{
    public class CopyActivity
    {
        [Key]
        public int CopyActivityId { get; set; }

        public string Name { get; set; }

        public virtual CopyActivitySource Source { get; set; }
        public int? CopyActivitySourceId { get; set; }

        public virtual CopyActivitySink Sink { get; set; }
        public int? CopyActivitySinkId { get; set; }

    }
}
