namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class ReportGeneratorOptions
    {
        public string Reports { get; set; } = null!;
        public string ReportTypes { get; set; } = null!;
        public string[]? ExtraArguments { get; set; }
    }
}
