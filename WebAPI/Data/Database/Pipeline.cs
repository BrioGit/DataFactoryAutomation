namespace WebAPI.Data.Database
{
    public class Pipeline
    {
        public int PipelineId { get; set; }

        public string Name { get; set; }

        public virtual IEnumerable<CopyActivity> CopyActivities { get; set; }

        public int DataFactoryId { get; set; }

    }
}
