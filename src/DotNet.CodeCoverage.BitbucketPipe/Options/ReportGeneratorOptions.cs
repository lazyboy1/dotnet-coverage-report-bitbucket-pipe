using System;
using DotNet.CodeCoverage.BitbucketPipe.Utils;

namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class ReportGeneratorOptions
    {
        public string Reports { get; set; } = null!;
        public string ReportTypes { get; set; } = null!;
        public string[]? ExtraArguments { get; set; }

        public static void Configure(ReportGeneratorOptions options)
        {
            string? reportTypes = Environment.GetEnvironmentVariable("REPORT_TYPES");
            options.ReportTypes = reportTypes ?? "JsonSummary;Html";

            string? reports = Environment.GetEnvironmentVariable("REPORTS");
            options.Reports = reports ?? "**/coverage*.xml";

            string? extraArgsCountString = Environment.GetEnvironmentVariable("EXTRA_ARGS_COUNT");
            int.TryParse(extraArgsCountString, out int extraArgsCount);
            if (extraArgsCount <= 0) {
                return;
            }

            options.ExtraArguments = new string[extraArgsCount];
            for (int i = 0; i < extraArgsCount; i++) {
                options.ExtraArguments[i] = EnvironmentUtils.GetRequiredEnvironmentVariable($"EXTRA_ARGS_{i}");
            }
        }
    }
}
