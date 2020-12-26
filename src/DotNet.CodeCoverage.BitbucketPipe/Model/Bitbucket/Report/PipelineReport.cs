using System;
using System.Collections.Generic;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.Report
{
    [Serializable]
    public class PipelineReport
    {
        public string? Uuid { get; set; }
        public string? Title { get; set; }
        public string? Details { get; set; }
        public string? ExternalId { get; set; }
        public string? Reporter { get; set; }
        public Uri? Link { get; set; }
        public ReportType ReportType { get; set; }
        public Result Result { get; set; }
        public ICollection<ReportDataItem> Data { get; set; } = new List<ReportDataItem>();
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }
}
