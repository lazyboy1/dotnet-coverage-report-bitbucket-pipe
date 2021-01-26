using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class PublishReportOptions
    {
        public Uri? ReportUrl { get; set; }

        public static void Configure(PublishReportOptions options)
        {
            string? reportUrlStr = Environment.GetEnvironmentVariable("PUBLISHED_REPORT_URL");
            Uri.TryCreate(reportUrlStr, UriKind.Absolute, out var reportUrl);
            options.ReportUrl = reportUrl;
        }
    }
}
