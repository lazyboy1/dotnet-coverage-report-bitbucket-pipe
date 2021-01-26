using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Options
{
    public class CoverageRequirementsOptions
    {
        public int LineCoveragePercentageMinimum { get; set; }
        public int BranchCoveragePercentageMinimum { get; set; }

        public static void Configure(CoverageRequirementsOptions options)
        {
            string? lineCoverageString = Environment.GetEnvironmentVariable("LINE_COVERAGE_MINIMUM");
            string? branchCoverageString = Environment.GetEnvironmentVariable("BRANCH_COVERAGE_MINIMUM");

            int.TryParse(lineCoverageString, out int lineCoverageMinimum);
            int.TryParse(branchCoverageString, out int branchCoverageMinimum);

            options.LineCoveragePercentageMinimum = lineCoverageMinimum;
            options.BranchCoveragePercentageMinimum = branchCoverageMinimum;
        }
    }
}
