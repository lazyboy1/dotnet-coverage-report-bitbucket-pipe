using System;
using JetBrains.Annotations;

namespace DotNet.CodeCoverage.BitbucketPipe.Utils
{
    [PublicAPI]
    public class ProcessErroredException : Exception
    {
        public string Executable { get; set; }
        public string Arguments { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string ErrorOutput { get; set; }

        public ProcessErroredException(string executable, string arguments, int exitCode, string output, string errorOutput)
        :base($"Process [{executable} {arguments}] exited with code {exitCode}. " +
              $"Output: {output}. Error: {errorOutput}")
        {
            Executable = executable;
            Arguments = arguments;
            ExitCode = exitCode;
            Output = output;
            ErrorOutput = errorOutput;
        }
    }
}
