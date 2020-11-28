using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Model.Bitbucket.CommitStatuses;
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
        private readonly PublishReportOptions _publishOptions;
        private readonly CoverageRequirementsOptions _requirementsOptions;
        private string Workspace { get; } = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_WORKSPACE");
        private string RepoSlug { get; } = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_REPO_SLUG");

        private string CommitHash { get; }

        public BitbucketClient(HttpClient client, ILogger<BitbucketClient> logger,
            IOptions<PublishReportOptions> publishOptions, IOptions<CoverageRequirementsOptions> requirementsOptions)
        {
            _httpClient = client;
            _logger = logger;
            _publishOptions = publishOptions.Value;
            _requirementsOptions = requirementsOptions.Value;

            CommitHash = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_COMMIT");

            // when using the proxy in an actual pipelines environment, requests must be sent over http

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.BaseAddress =
                new Uri(
                    $"https://api.bitbucket.org/2.0/repositories/{Workspace}/{RepoSlug}/commit/{CommitHash}/");

            _logger.LogDebug("Base address: {baseAddress}", client.BaseAddress);
        }

        public async Task CreateCommitBuildStatusAsync(CoverageSummary summary)
        {
            _logger.LogDebug("Coverage requirements: {@CoverageRequirements}", _requirementsOptions);
            _logger.LogDebug("Coverage summary: {@CoverageSummary}", summary);

            bool meetsRequirements =
                _requirementsOptions.BranchCoveragePercentageMinimum <= summary.BranchCoveragePercentage &&
                _requirementsOptions.LineCoveragePercentageMinimum <= summary.LineCoveragePercentage;

            _logger.LogDebug("Coverage meets requirements? {meetsRequirements}", meetsRequirements);

            const string key = "Code-Coverage";
            const string name = "Code Coverage Report";
            var state = meetsRequirements ? State.Successful : State.Failed;
            var buildStatus = new BuildStatus(key, name, state, Workspace, RepoSlug, _publishOptions.ReportUrl)
                {Description = state == State.Failed ? "Coverage doesn't meet requirements" : ""};

            string serializedBuildStatus = Serialize(buildStatus);

            _logger.LogDebug("POSTing build status: {buildStatus}", serializedBuildStatus);

            var response = await _httpClient.PostAsync("statuses/build", CreateStringContent(serializedBuildStatus));
            await VerifyResponseAsync(response);
        }

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
            _logger.LogDebug("Response status code: {statusCode}", (int) response.StatusCode);
            if (!response.IsSuccessStatusCode) {
                string error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error response: {error}", error);
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
