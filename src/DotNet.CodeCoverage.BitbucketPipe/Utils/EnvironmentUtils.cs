using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Utils
{
    internal static class EnvironmentUtils
    {
        public static string GetRequiredEnvironmentVariable(string variableName) =>
            Environment.GetEnvironmentVariable(variableName) ??
            throw new RequiredEnvironmentVariableNotFoundException(variableName);

        // ReSharper disable once MemberCanBePrivate.Global
        public static string EnvironmentName { get; } =
            Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production";

        // ReSharper disable once UnusedMember.Global
        public static bool IsDevelopment { get; } =
            EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
    }
}
