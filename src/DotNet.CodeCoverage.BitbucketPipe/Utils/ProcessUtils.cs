using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace DotNet.CodeCoverage.BitbucketPipe.Utils
{
    public static class ProcessUtils
    {
        public static void RunProcessUntilFinishedOrCanceled(string executable, string arguments,
            ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            var processStartInfo = new ProcessStartInfo(executable, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using var process = new Process {StartInfo = processStartInfo};
            cancellationToken.ThrowIfCancellationRequested();

            logger?.LogDebug("Executing command: {Command}", $"{executable} {arguments}");
            process.Start();

            while (!process.HasExited && !cancellationToken.IsCancellationRequested) {
                process.WaitForExit(1000);
            }

            if (cancellationToken.IsCancellationRequested) {
                process.Kill();
                cancellationToken.ThrowIfCancellationRequested();
            }

            logger?.LogDebug("Exit code: {ExitCode}", process.ExitCode);
            if (process.ExitCode != 0) {
                throw new ProcessErroredException(process.StartInfo.FileName, process.StartInfo.Arguments,
                    process.ExitCode, "", "");
            }
        }
    }
}
