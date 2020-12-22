using System;
using DotNet.CodeCoverage.BitbucketPipe.Utils;

namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    internal static class OptionsConfigurator
    {
        public static void ConfigurePublishReportOptions(PublishReportOptions options)
        {
            string? reportUrlStr = Environment.GetEnvironmentVariable("PUBLISHED_REPORT_URL");
            Uri.TryCreate(reportUrlStr, UriKind.Absolute, out var reportUrl);
            options.ReportUrl = reportUrl;
        }

        public static void ConfigureCoverageRequirementsOptions(CoverageRequirementsOptions options)
        {
            string? lineCoverageString = Environment.GetEnvironmentVariable("LINE_COVERAGE_MINIMUM");
            string? branchCoverageString = Environment.GetEnvironmentVariable("BRANCH_COVERAGE_MINIMUM");

            int.TryParse(lineCoverageString, out int lineCoverageMinimum);
            int.TryParse(branchCoverageString, out int branchCoverageMinimum);

            options.LineCoveragePercentageMinimum = lineCoverageMinimum;
            options.BranchCoveragePercentageMinimum = branchCoverageMinimum;
        }

        public static void ConfigureReportGeneratorOptions(ReportGeneratorOptions options)
        {
            string? extraArguments = Environment.GetEnvironmentVariable("EXTRA_ARGS");
            if (string.IsNullOrWhiteSpace(extraArguments)) {
                return;
            }

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
