﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Model;
using DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.CommitStatuses;
using DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.Report;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using DotNet.CodeCoverage.BitbucketPipe.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNet.CodeCoverage.BitbucketPipe
{
    public class BitbucketClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BitbucketClient> _logger;
        private readonly BitbucketOptions _bitbucketOptions;
        private readonly PublishReportOptions _publishOptions;
        private readonly CoverageRequirementsOptions _requirementsOptions;

        public BitbucketClient(HttpClient client, ILogger<BitbucketClient> logger,
            IOptions<PublishReportOptions> publishOptions, IOptions<CoverageRequirementsOptions> requirementsOptions,
            IOptions<BitbucketOptions> bitbucketOptions)
        {
            _httpClient = client;
            _logger = logger;
            _bitbucketOptions = bitbucketOptions.Value;
            _publishOptions = publishOptions.Value;
            _requirementsOptions = requirementsOptions.Value;

            CommitHash = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_COMMIT");

            // when using the proxy in an actual pipelines environment, requests must be sent over http

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.BaseAddress =
                new Uri(
                    $"https://api.bitbucket.org/2.0/repositories/{Workspace}/{RepoSlug}/commit/{CommitHash}/");

            _logger.LogDebug("Base address: {BaseAddress}", client.BaseAddress);
        }

        private string Workspace { get; } = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_WORKSPACE");
        private string RepoSlug { get; } = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_REPO_SLUG");
        private string CommitHash { get; }

        public async Task CreateCommitBuildStatusAsync(CoverageSummary summary)
        {
            _logger.LogDebug("Coverage requirements: {@CoverageRequirements}", _requirementsOptions);
            _logger.LogDebug("Coverage summary: {@CoverageSummary}", summary);

            bool meetsRequirements = CoverageMeetsRequirements(summary);

            _logger.LogDebug("Coverage meets requirements? {MeetsRequirements}", meetsRequirements);

            const string key = "Code-Coverage";
            var state = meetsRequirements ? State.Successful : State.Failed;
            var buildStatus = new BuildStatus(key, _bitbucketOptions.BuildStatusName, state, Workspace, RepoSlug,
                    _publishOptions.ReportUrl)
                {Description = state == State.Failed ? "Coverage doesn't meet requirements" : ""};

            string serializedBuildStatus = Serialize(buildStatus);

            _logger.LogDebug("POSTing build status: {BuildStatus}", serializedBuildStatus);

            var response = await _httpClient.PostAsync("statuses/build", CreateStringContent(serializedBuildStatus));
            await VerifyResponseAsync(response);
        }

        public async Task CreateReportAsync(CoverageSummary summary)
        {
            var pipelineReport = new PipelineReport
            {
                Title = _bitbucketOptions.ReportTitle,
                Details = "Line and branch coverage summary",
                Link = _publishOptions.ReportUrl,
                ExternalId = "code-coverage",
                ReportType = ReportType.Coverage,
                Result = CoverageMeetsRequirements(summary) ? Result.Passed : Result.Failed,
                Data =
                {
                    new ReportDataItem
                    {
                        Title = "Line Coverage", Type = ReportDataType.Percentage,
                        Value = summary.LineCoveragePercentage
                    },
                    new ReportDataItem
                    {
                        Title = "Branch Coverage", Type = ReportDataType.Percentage,
                        Value = summary.BranchCoveragePercentage
                    }
                }
            };

            string serializedReport = Serialize(pipelineReport);

            _logger.LogDebug("PUTing report: {Report}", serializedReport);

            var response = await _httpClient.PutAsync($"reports/{pipelineReport.ExternalId}",
                CreateStringContent(serializedReport));
            await VerifyResponseAsync(response);
        }

        private bool CoverageMeetsRequirements(CoverageSummary summary) =>
            _requirementsOptions.BranchCoveragePercentageMinimum <= summary.BranchCoveragePercentage &&
            _requirementsOptions.LineCoveragePercentageMinimum <= summary.LineCoveragePercentage;

        private static string Serialize(object obj)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }

        private static StringContent CreateStringContent(string str) =>
            new StringContent(str, Encoding.Default, "application/json");

        private async Task VerifyResponseAsync(HttpResponseMessage response)
        {
            _logger.LogDebug("Response status code: {StatusCode}", (int) response.StatusCode);
            if (!response.IsSuccessStatusCode) {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error response: {Error}", error);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
