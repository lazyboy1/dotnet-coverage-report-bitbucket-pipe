using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class BitbucketOptions
    {
        public string ReportTitle { get; set; } = null!;
        public string BuildStatusName { get; set; } = null!;

        public static void Configure(BitbucketOptions options)
        {
            string? reportTitle = Environment.GetEnvironmentVariable("PIPELINE_REPORT_TITLE");
            string? buildStatusName = Environment.GetEnvironmentVariable("BUILD_STATUS_NAME");

            if (string.IsNullOrWhiteSpace(reportTitle) && !string.IsNullOrWhiteSpace(buildStatusName)) {
                reportTitle = buildStatusName;
            }
            else if (string.IsNullOrWhiteSpace(buildStatusName) && !string.IsNullOrWhiteSpace(reportTitle)) {
                buildStatusName = reportTitle;
            }

            const string defaultTitle = "Code Coverage";
            options.BuildStatusName = buildStatusName ?? defaultTitle;
            options.ReportTitle = reportTitle ?? defaultTitle;
        }
    }
}
