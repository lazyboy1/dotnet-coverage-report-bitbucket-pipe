using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Model;
using DotNet.CodeCoverage.BitbucketPipe.Tests.BDD;
using Moq;
using Moq.Protected;
using static DotNet.CodeCoverage.BitbucketPipe.Utils.EnvironmentUtils;

namespace DotNet.CodeCoverage.BitbucketPipe.Tests.BitbucketClientTests
{
    public class When_Making_A_Request_To_Create_Report : BitbucketClientSpecificationBase
    {
        protected override async Task WhenAsync()
        {
            await base.WhenAsync();

            var coverageSummary = new CoverageSummary {BranchCoveragePercentage = 85, LineCoveragePercentage = 85};
            await BitbucketClient.CreateReportAsync(coverageSummary);
        }

        [Then]
        public void It_Should_Make_One_Put_Call_To_Create_Report()
        {
            string commit = GetRequiredEnvironmentVariable("BITBUCKET_COMMIT");
            string repoSlug = GetRequiredEnvironmentVariable("BITBUCKET_REPO_SLUG");
            string workspace = GetRequiredEnvironmentVariable("BITBUCKET_WORKSPACE");

            HttpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(message =>
                    message.Method == HttpMethod.Put &&
                    message.RequestUri.PathAndQuery.EndsWith(
                        $"{workspace}/{repoSlug}/commit/{commit}/reports/coverage-report")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
