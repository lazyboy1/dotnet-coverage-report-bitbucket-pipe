using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNet.CodeCoverage.BitbucketPipe.Options;
using DotNet.CodeCoverage.BitbucketPipe.Tests.BDD;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DotNet.CodeCoverage.BitbucketPipe.Tests.BitbucketClientTests
{
    public class BitbucketClientSpecificationBase : SpecificationBase
    {
        protected Mock<HttpMessageHandler> HttpMessageHandlerMock { get; private set; }
        protected BitbucketClient BitbucketClient { get; private set; }

        protected override void Given()
        {
            base.Given();

            TestEnvironmentSetup.SetupEnvironment();

            HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
            HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var httpClient = new HttpClient(HttpMessageHandlerMock.Object);

            var publishReportOptions =
                Mock.Of<IOptions<PublishReportOptions>>(options => options.Value == new PublishReportOptions());

            var requirementsOptions = Mock.Of<IOptions<CoverageRequirementsOptions>>(options => options.Value ==
                new CoverageRequirementsOptions
                    {BranchCoveragePercentageMinimum = 80, LineCoveragePercentageMinimum = 80});

            BitbucketClient = new BitbucketClient(httpClient, NullLogger<BitbucketClient>.Instance,
                publishReportOptions,
                requirementsOptions);
        }
    }
}
