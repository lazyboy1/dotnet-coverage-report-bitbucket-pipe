using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.Report
{
    [Serializable]
    public class ReportDataItem
    {
        public ReportDataType Type { get; set; }
        public string? Title { get; set; }
        public object? Value { get; set; }
    }
}
