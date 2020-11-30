using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Tests
{
    public static class TestEnvironmentSetup
    {
        public static void SetupEnvironment()
        {
            Environment.SetEnvironmentVariable("BITBUCKET_WORKSPACE", "Workspace");
            Environment.SetEnvironmentVariable("BITBUCKET_REPO_SLUG", "repo-slug");
            Environment.SetEnvironmentVariable("BITBUCKET_COMMIT", "222be690");
            Environment.SetEnvironmentVariable("BITBUCKET_OAUTH_KEY", "oauth-key");
            Environment.SetEnvironmentVariable("BITBUCKET_OAUTH_SECRET", "oauth-secret");
            Environment.SetEnvironmentVariable("LINE_COVERAGE_MINIMUM", "80");
            Environment.SetEnvironmentVariable("BRANCH_COVERAGE_MINIMUM", "80");
            Environment.SetEnvironmentVariable("PUBLISHED_REPORT_URL", "");

        }
    }
}
