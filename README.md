﻿# Bitbucket Pipelines Pipe: .NET Coverage Report Pipe

Generate coverage reports for .NET apps
with [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
and a build status based on coverage requirements.

## YAML Definition

Add the following snippet to the script section of
your `bitbucket-pipelines.yml` file:

```yaml
script:
  - pipe: docker://lazyboy1/dotnet-coverage-report-bitbucket-pipe:0.3
    variables:
      BITBUCKET_OAUTH_KEY: "<string>"
      BITBUCKET_OAUTH_SECRET: "<string>"
      # LINE_COVERAGE_MINIMUM: "<int>" # Optional, default: 0
      # BRANCH_COVERAGE_MINIMUM: "<int>" # Optional, default: 0
      # PUBLISHED_REPORT_URL: "<string>" # Optional
      # REPORTS: "<string>" # Optional, default: "**/coverage*.xml"
      # REPORT_TYPES: "<string>" # Optional, default: "JsonSummary;Html"
      # EXTRA_ARGS: ['arg1', 'arg2'] # Optional
      # DEBUG: "<boolean>" # Optional
```

## Variables

| Variable | Usage |
| -------- | ----- |
| BITBUCKET_OAUTH_KEY (\*)    | OAuth consumer key |
| BITBUCKET_OAUTH_SECRET (\*) | OAuth consumer secret |
| REPORTS                     | Path(s) to coverage reports. Supports globbing. Passed as value of `-reports` argument to `reportgenerator`. Default: `**/coverage*.xml` |
| REPORT_TYPES                | The types of reports to generate. Passed as value of `-reporttypes` argument to `reportgenerator`. Default: `JsonSummary;Html`. Note: JsonSummary is required to enforce minimum requirements. |
| LINE_COVERAGE_MINIMUM       | Minimum requirement for line coverage percentage. Default: `0` |
| BRANCH_COVERAGE_MINIMUM     | Minimum requirement for branch coverage percentage. Default: `0` |
| PUBLISHED_REPORT_URL        | If you intend to upload coverage report generated by this pipe, this should contain the URL to the published report. |
| EXTRA_ARGS                  | Extra arguments array that are passed as is to `reportgenerator` |
| DEBUG                       | Turn on extra debug information. Default: `false` |

_(\*) = required variable._

## Prerequisites

### OAuth Consumer

OAuth consumer configuration:

1. Set a callback URL - you can use your Bitbucket workspace URL.
1. Check the "This is a private consumer" checkbox to
   enable `client_credentials`.
2. Allow Repository write permissions

More details about Bitbucket OAuth consumers in their
[docs.](https://support.atlassian.com/bitbucket-cloud/docs/use-oauth-on-bitbucket-cloud/#OAuthonBitbucketCloud-Createaconsumer)

### Code Coverage Files

Code coverage files need to be present when the pipe runs. It is assumed that
coverage is collected
using [Coverlet](https://github.com/coverlet-coverage/coverlet) with default
settings. The easiest way to set up code coverage collection:

1. Install NuGet
   package [coverlet.collector](https://www.nuget.org/packages/coverlet.collector/)
   in all test projects:

   ```
   dotnet add package coverlet.collector
   ```

2. Run the following command:

   ```
   dotnet test --collect:"XPlat Code Coverage"
   ```

## Uploading The Report

Report files are generated in folder `./coverage-report`. You can use another
pipe to upload the report files to your preferred storage provider. If you
provide the expected report URL in `PUBLISHED_REPORT_URL` variable, it will be
used as the link for the build status for easy access. Depending on your storage
provider, you may need to link directly to the `index.html` file in order to be
able browse to it.

## Examples

Basic example using secure variables for OAuth:

```yaml
script:
  - pipe: docker://lazyboy1/dotnet-coverage-report-bitbucket-pipe:0.3
    variables:
      BITBUCKET_OAUTH_KEY: $OAUTH_KEY
      BITBUCKET_OAUTH_SECRET: $OAUTH_SECRET
      LINE_COVERAGE_MINIMUM: "80"
      BRANCH_COVERAGE_MINIMUM: "80"
```

Example with published report URL:

```yaml
script:
  - pipe: docker://lazyboy1/dotnet-coverage-report-bitbucket-pipe:0.3
    variables:
      BITBUCKET_OAUTH_KEY: $OAUTH_KEY
      BITBUCKET_OAUTH_SECRET: $OAUTH_SECRET
      LINE_COVERAGE_MINIMUM: "80"
      BRANCH_COVERAGE_MINIMUM: "80",
      PUBLISHED_REPORT_URL: "https://my-server.com/coverage/$BITBUCKET_REPO_SLUG/$BITBUCKET_COMMIT/index.html"
```

Example with extra arguments:

```yaml
script:
  - pipe: docker://lazyboy1/dotnet-coverage-report-bitbucket-pipe:0.3
    variables:
      BITBUCKET_OAUTH_KEY: $OAUTH_KEY
      BITBUCKET_OAUTH_SECRET: $OAUTH_SECRET
      LINE_COVERAGE_MINIMUM: "80"
      BRANCH_COVERAGE_MINIMUM: "80"
      REPORTS: "coverage/cobertura.xml"
      REPORT_TYPES: "JsonSummary;HtmlSummary"
      EXTRA_ARGS: ['-plugins:my-history-storage-plugin.dll', '"-classfilters:+IncludeThisClass;-ExcludeThisClass"']
      PUBLISHED_REPORT_URL: "https://my-server.com/coverage/$BITBUCKET_REPO_SLUG/$BITBUCKET_COMMIT/summary.html"

```

## Support

If you're reporting an issue, please include:

- the version of the pipe
- relevant logs and error messages
- steps to reproduce

## License

[MIT License](LICENSE)
