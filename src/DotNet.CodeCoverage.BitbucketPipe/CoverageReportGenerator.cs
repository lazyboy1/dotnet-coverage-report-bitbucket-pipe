using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Model;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using DotNet.CodeCoverage.BitbucketPipe.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNet.CodeCoverage.BitbucketPipe
{
    public class CoverageReportGenerator
    {
        private readonly ILogger<CoverageReportGenerator> _logger;
        private readonly ReportGeneratorOptions _options;
        private readonly string _coverageReportPath;

        public CoverageReportGenerator(ILogger<CoverageReportGenerator> logger,
            IOptions<ReportGeneratorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
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

        private void RunCoverageReportGenerator()
        {
            string argumentsString = PrepareCommandArguments();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var cancellationToken = cancellationTokenSource.Token;
            ProcessUtils.RunProcessUntilFinishedOrCanceled("reportgenerator", argumentsString,
                _logger, cancellationToken);
        }

        private string PrepareCommandArguments()
        {
            string[] basicArguments =
            {
                "\"-reports:**/coverage*.xml\"",
                $"-targetdir:{_coverageReportPath}",
                "-reporttypes:JsonSummary;Html"
            };
            string[] allArguments;
            if (_options.ExtraArguments != null && _options.ExtraArguments.Length > 0) {
                allArguments = basicArguments.Concat(_options.ExtraArguments).ToArray();
            }
            else {
                allArguments = basicArguments;
            }

            string argumentsString = string.Join(' ', allArguments);
            return argumentsString;
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
