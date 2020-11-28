using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using DotNet.CodeCoverage.BitbucketPipe.Utils;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DotNet.CodeCoverage.BitbucketPipe
{
    internal static class Program
    {
        private static async Task Main()
        {
            bool isDebug =
                Environment.GetEnvironmentVariable("DEBUG")?.Equals("true", StringComparison.OrdinalIgnoreCase)
                ?? false;
            Log.Logger = LoggerInitializer.CreateLogger(isDebug);

            Log.Logger.Debug("DEBUG={isDebug}", isDebug);

            var serviceProvider = await ConfigureServicesAsync();

            var coverageReportGenerator = serviceProvider.GetRequiredService<CoverageReportGenerator>();
            var coverageSummary = await coverageReportGenerator.GenerateCoverageReportAsync();

            var bitbucketClient = serviceProvider.GetRequiredService<BitbucketClient>();
            await bitbucketClient.CreateCommitBuildStatusAsync(coverageSummary);
        }

        private static async Task<ServiceProvider> ConfigureServicesAsync()
        {
            string accessToken = await GetAccessTokenAsync();

            var serviceCollection =
                new ServiceCollection()
                    .AddSingleton<CoverageReportGenerator>()
                    .AddHttpClient<BitbucketClient>(client =>
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(
                                OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer,
                                accessToken)).Services
                    .AddLogging(builder => builder.AddSerilog())
                    .Configure<CoverageRequirementsOptions>(ConfigureCoverageRequirementsOptions)
                    .Configure<PublishReportOptions>(ConfigurePublishReportOptions);

            return serviceCollection.BuildServiceProvider();
        }

        private static void ConfigurePublishReportOptions(PublishReportOptions options)
        {
            string? reportUrlStr = Environment.GetEnvironmentVariable("PUBLISHED_REPORT_URL");
            Uri.TryCreate(reportUrlStr, UriKind.Absolute, out var reportUrl);
            options.ReportUrl = reportUrl;
        }

        private static void ConfigureCoverageRequirementsOptions(CoverageRequirementsOptions options)
        {
            string? lineCoverageString = Environment.GetEnvironmentVariable("LINE_COVERAGE_MINIMUM");
            string? branchCoverageString = Environment.GetEnvironmentVariable("BRANCH_COVERAGE_MINIMUM");

            int.TryParse(lineCoverageString, out int lineCoverageMinimum);
            int.TryParse(branchCoverageString, out int branchCoverageMinimum);

            options.LineCoveragePercentageMinimum = lineCoverageMinimum;
            options.BranchCoveragePercentageMinimum = branchCoverageMinimum;
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            var authenticationOptions = new BitbucketAuthenticationOptions
            {
                Key = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_OAUTH_KEY"),
                Secret = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_OAUTH_SECRET")
            };

            Log.Logger.Debug("Getting access token...");

            using var httpClient = new HttpClient();
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = authenticationOptions.Key,
                ClientSecret = authenticationOptions.Secret,
                Scope = "repository:write",
                Address = "https://bitbucket.org/site/oauth2/access_token"
            };

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(tokenRequest);

            if (!tokenResponse.IsError) {
                Log.Logger.Debug("Got access token");
                return tokenResponse.AccessToken;
            }

            Log.Logger.Error("Error getting access token: {@error}",
                new
                {
                    tokenResponse.Error, tokenResponse.ErrorDescription, tokenResponse.ErrorType,
                    tokenResponse.HttpErrorReason, tokenResponse.HttpStatusCode
                });

            throw new OAuthException(tokenResponse);
        }
    }
}
