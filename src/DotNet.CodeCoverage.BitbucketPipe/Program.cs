using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using static System.Environment;
using static DotNet.CodeCoverage.BitbucketPipe.Utils.EnvironmentUtils;

namespace DotNet.CodeCoverage.BitbucketPipe
{
    internal static class Program
    {
        private static async Task Main()
        {
            Log.Logger = LoggerInitializer.CreateLogger(IsDebugMode);

            Log.Debug("DEBUG={IsDebug}", IsDebugMode);
            Log.Debug("Workdir={Workdir}", CurrentDirectory);

            var serviceProvider = await ConfigureServicesAsync();

            var coverageReportGenerator = serviceProvider.GetRequiredService<CoverageReportGenerator>();
            var coverageSummary = await coverageReportGenerator.GenerateCoverageReportAsync();

            var bitbucketClient = serviceProvider.GetRequiredService<BitbucketClient>();
            await bitbucketClient.CreateCommitBuildStatusAsync(coverageSummary);
            await bitbucketClient.CreateReportAsync(coverageSummary);
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
                                accessToken))
                    .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandlerForTests).Services
                    .AddLogging(builder => builder.AddSerilog())
                    .Configure<CoverageRequirementsOptions>(CoverageRequirementsOptions.Configure)
                    .Configure<PublishReportOptions>(PublishReportOptions.Configure)
                    .Configure<ReportGeneratorOptions>(ReportGeneratorOptions.Configure)
                    .Configure<BitbucketOptions>(BitbucketOptions.Configure);

            return serviceCollection.BuildServiceProvider();
        }

        private static HttpMessageHandler ConfigureHttpMessageHandlerForTests() => new HttpClientHandler
        {
            // ignore SSL errors in tests
            ServerCertificateCustomValidationCallback = (request, x509Certificate2, x509Chain, sslPolicyErrors) =>
                EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase) ||
                sslPolicyErrors == SslPolicyErrors.None
        };

        private static async Task<string> GetAccessTokenAsync()
        {
            var authenticationOptions = new BitbucketAuthenticationOptions
            {
                Key = GetRequiredEnvironmentVariable("BITBUCKET_OAUTH_KEY"),
                Secret = GetRequiredEnvironmentVariable("BITBUCKET_OAUTH_SECRET")
            };

            Log.Debug("Getting access token...");

            using var httpClient = new HttpClient(ConfigureHttpMessageHandlerForTests());
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = authenticationOptions.Key,
                ClientSecret = authenticationOptions.Secret,
                Scope = "repository:write",
                Address = "https://bitbucket.org/site/oauth2/access_token"
            };

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(tokenRequest);

            if (!tokenResponse.IsError) {
                Log.Debug("Got access token");
                return tokenResponse.AccessToken;
            }

            Log.Error("Error getting access token: {@Error}",
                new
                {
                    tokenResponse.Error, tokenResponse.ErrorDescription, tokenResponse.ErrorType,
                    tokenResponse.HttpStatusCode
                });

            throw new OAuthException(tokenResponse);
        }
    }
}
