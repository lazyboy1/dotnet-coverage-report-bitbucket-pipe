using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Utils;
using Microsoft.Extensions.Logging;

namespace DotNet.CodeCoverage.BitbucketPipe
{
    public class CoverageReportGenerator
    {
        private readonly ILogger<CoverageReportGenerator> _logger;
        private readonly string _coverageReportPath;

        public CoverageReportGenerator(ILogger<CoverageReportGenerator> logger)
        {
            _logger = logger;
            _coverageReportPath = "coverage-report";
        }

        public async Task<CoverageSummary> GenerateCoverageReportAsync()
        {
            RunCoverageReportGenerator();
            try {
                return await ParseCoverageSummaryAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error parsing summary report");
                return new CoverageSummary();
            }
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private void RunCoverageReportGenerator()
        {
            string reportGeneratorArguments =
                "\"-reports:**/coverage*.xml\" " +
                $"-targetdir:{_coverageReportPath} " +
                "-reporttypes:JsonSummary;Html";

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var cancellationToken = cancellationTokenSource.Token;
            ProcessUtils.RunProcessUntilFinishedOrCanceled("reportgenerator", reportGeneratorArguments,
                _logger, cancellationToken);
        }

        private async Task<CoverageSummary> ParseCoverageSummaryAsync()
        {
            await using var fileStream = File.OpenRead(Path.Combine(_coverageReportPath, "Summary.json"));

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = cancellationTokenSource.Token;

            using var jsonDocument = await JsonDocument.ParseAsync(fileStream, cancellationToken: cancellationToken);
            var summaryElement = jsonDocument.RootElement.GetProperty("summary");
            var summaryEnumerator = summaryElement.EnumerateObject();
            var coverageSummary = new CoverageSummary
            {
                // ReSharper disable  StringLiteralTypo
                LineCoveragePercentage = summaryEnumerator.First(_ => _.NameEquals("linecoverage")).Value.GetDouble(),
                BranchCoveragePercentage =
                    summaryEnumerator.First(_ => _.NameEquals("branchcoverage")).Value.GetDouble()
                // ReSharper restore  StringLiteralTypo
            };

            return coverageSummary;
        }
    }
}
