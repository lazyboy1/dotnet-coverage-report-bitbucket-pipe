using static System.Environment;

namespace DotNet.CodeCoverage.BitbucketPipe.Tests
{
    public static class TestEnvironmentSetup
    {
        public static void SetupEnvironment()
        {
            SetEnvironmentVariable("BITBUCKET_WORKSPACE", "Workspace");
            SetEnvironmentVariable("BITBUCKET_REPO_SLUG", "repo-slug");
            SetEnvironmentVariable("BITBUCKET_COMMIT", "222be690");
            SetEnvironmentVariable("BITBUCKET_OAUTH_KEY", "oauth-key");
            SetEnvironmentVariable("BITBUCKET_OAUTH_SECRET", "oauth-secret");
            SetEnvironmentVariable("LINE_COVERAGE_MINIMUM", "80");
            SetEnvironmentVariable("BRANCH_COVERAGE_MINIMUM", "80");
            SetEnvironmentVariable("PUBLISHED_REPORT_URL", "");

        }
    }
}
