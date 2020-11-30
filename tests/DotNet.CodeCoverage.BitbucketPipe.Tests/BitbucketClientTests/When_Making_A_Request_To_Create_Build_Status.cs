using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Model;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using DotNet.CodeCoverage.BitbucketPipe.Tests.BDD;
using DotNet.CodeCoverage.BitbucketPipe.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DotNet.CodeCoverage.BitbucketPipe.Tests.BitbucketClientTests
{
    public class When_Generating_Coverage_Report : SpecificationBase
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private BitbucketClient _bitbucketClient;

        protected override void Given()
        {
            base.Given();

            TestEnvironmentSetup.SetupEnvironment();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            // var authOptions = Mock.Of<IOptions<BitbucketAuthenticationOptions>>(options =>
            //     options.Value == new BitbucketAuthenticationOptions {Key = "", Secret = ""});

            var publishReportOptions =
                Mock.Of<IOptions<PublishReportOptions>>(options => options.Value == new PublishReportOptions());

            var requirementsOptions = Mock.Of<IOptions<CoverageRequirementsOptions>>(options => options.Value ==
                new CoverageRequirementsOptions
                    {BranchCoveragePercentageMinimum = 80, LineCoveragePercentageMinimum = 80});

            _bitbucketClient = new BitbucketClient(httpClient, NullLogger<BitbucketClient>.Instance,
                publishReportOptions,
                requirementsOptions);
        }

        protected override async Task WhenAsync()
        {
            await base.WhenAsync();

            await _bitbucketClient.CreateCommitBuildStatusAsync(new CoverageSummary
                {BranchCoveragePercentage = 85, LineCoveragePercentage = 85});
        }

        [Then]
        public void It_Should_Make_One_Post_Call_To_Create_Build_Status()
        {
            string commit = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_COMMIT");
            string repoSlug = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_REPO_SLUG");
            string workspace = EnvironmentUtils.GetRequiredEnvironmentVariable("BITBUCKET_WORKSPACE");

            _httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(message =>
                    message.Method == HttpMethod.Post &&
                    message.RequestUri.PathAndQuery.EndsWith(
                        $"{workspace}/{repoSlug}/commit/{commit}/statuses/build")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
