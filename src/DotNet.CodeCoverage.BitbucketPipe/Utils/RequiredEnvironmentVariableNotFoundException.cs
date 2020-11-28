using System;

namespace DotNet.CodeCoverage.BitbucketPipe.Utils
{
    public class RequiredEnvironmentVariableNotFoundException : Exception
    {
        public RequiredEnvironmentVariableNotFoundException(string variableName) :
            base($"Required environment variable {variableName} not found")
        {
        }
    }
}
