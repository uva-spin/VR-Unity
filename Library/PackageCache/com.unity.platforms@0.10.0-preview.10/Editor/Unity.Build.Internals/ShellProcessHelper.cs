using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.Build.Internals
{
    internal class ShellProcessOutput
    {
        public bool Succeeded { get; set; } = true;
        public string Command { get; set; }
        public StringBuilder CommandOutput { get; set; }
        public string FullOutput { get; set; }
        public string ErrorOutput { get; set; }
        public int ExitCode { get; set; }
    }

    internal class ShellProcessArguments
    {
        private const int k_DefaultMaxIdleTimeInMilliseconds = 30000;
        public string Executable { get; set; }
        public string[] Arguments { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }
        public IReadOnlyDictionary<string, string> EnvironmentVariables { get; set; }
        public int MaxIdleTimeInMilliseconds { get; set; } = k_DefaultMaxIdleTimeInMilliseconds;
        public bool MaxIdleKillIsAnError { get; set; } = true;
        public DataReceivedEventHandler OutputDataReceived { get; set; }
        public DataReceivedEventHandler ErrorDataReceived { get; set; }
        public bool ThrowOnError { get; set; } = true;
        public bool AnyOutputToErrorIsAnError { get; set; } = true;
    }

    internal enum ProcessStatus
    {
        Running,
        Killed,
        Done
    }

    internal struct ShellProcessProgressInfo
    {
        public Process Process { get; set; }
        public float Progress { get; set; }
        public string Info { get; set; }
        public StringBuilder Output { get; set; }
        public int ExitCode { get; set; }
    }

    internal class ShellProcess
    {
        public Process Process { get; private set; }
        public DateTime TimeOfLastObservedOutput { get; private set; }

        ShellProcess(ProcessStartInfo startInfo)
        {
            Process = new Process { StartInfo = startInfo };
            TimeOfLastObservedOutput = DateTime.Now;
        }

        public static ShellProcess Start(ShellProcessArguments args)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = args.Executable,
                Arguments = string.Join(" ", args.Arguments),
                WorkingDirectory = args.WorkingDirectory?.FullName ?? new DirectoryInfo(".").FullName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            if (args.EnvironmentVariables != null)
            {
                foreach (var pair in args.EnvironmentVariables)
                {
                    startInfo.EnvironmentVariables[pair.Key] = pair.Value;
                }
            }

            var shellProcess = new ShellProcess(startInfo);
            shellProcess.Process.OutputDataReceived += (sender, data) =>
            {
                shellProcess.TimeOfLastObservedOutput = DateTime.Now;
                args.OutputDataReceived?.Invoke(sender, data);
            };
            shellProcess.Process.ErrorDataReceived += (sender, data) =>
            {
                shellProcess.TimeOfLastObservedOutput = DateTime.Now;
                args.ErrorDataReceived?.Invoke(sender, data);
            };

            shellProcess.Process.Start();
            shellProcess.Process.BeginOutputReadLine();
            shellProcess.Process.BeginErrorReadLine();
            return shellProcess;
        }

        public IEnumerator<ProcessStatus> WaitForProcess(int maxIdleTimeInMs, int yieldFrequencyInMs = 100)
        {
            while (true)
            {
                if (Process.WaitForExit(yieldFrequencyInMs))
                {
                    // WaitForExit with a timeout will not wait for async event handling operations to finish.
                    // To ensure that async event handling has been completed, call WaitForExit that takes no parameters.
                    // See remarks: https://msdn.microsoft.com/en-us/library/ty0d8k56(v=vs.110)

                    Process.WaitForExit();
                    yield return ProcessStatus.Done;
                    break;
                }

                var IdleTimeInMs = (DateTime.Now - TimeOfLastObservedOutput).TotalMilliseconds;
                if (IdleTimeInMs < maxIdleTimeInMs || Debugger.IsAttached)
                {
                    yield return ProcessStatus.Running;
                    continue;
                }

                UnityEngine.Debug.LogError("Idle process detected. See console for more details.");
                Process.Kill();
                Process.WaitForExit();
                yield return ProcessStatus.Killed;
                break;
            }
        }

        public static ShellProcessOutput Run(ShellProcessArguments shellArgs)
        {
            Assert.IsNotNull(shellArgs);
            Assert.IsFalse(string.IsNullOrEmpty(shellArgs.Executable));

            var runOutput = new ShellProcessOutput();
            var hasErrors = false;
            var output = new StringBuilder();
            var logOutput = new StringBuilder();
            var errorOutput = new StringBuilder();

            // Prepare data received handlers
            DataReceivedEventHandler outputReceived = (sender, e) =>
            {
                LogProcessData(e.Data, output);
                logOutput.AppendLine(e.Data);
            };
            DataReceivedEventHandler errorReceived = (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                    if (shellArgs.AnyOutputToErrorIsAnError)
                    {
                        hasErrors = true;
                    }
                }
                LogProcessData(e.Data, output);
                logOutput.AppendLine(e.Data);
            };

            // Run command in shell and wait for exit
            var process = Start(new ShellProcessArguments
            {
                Executable = shellArgs.Executable,
                Arguments = shellArgs.Arguments,
                EnvironmentVariables = shellArgs.EnvironmentVariables,
                WorkingDirectory = shellArgs.WorkingDirectory,
                OutputDataReceived = outputReceived,
                ErrorDataReceived = errorReceived
            });
            var processUpdate = process.WaitForProcess(shellArgs.MaxIdleTimeInMilliseconds);
            while (processUpdate.MoveNext())
            {
            }

            var exitCode = process.Process.ExitCode;
            if (processUpdate.Current == ProcessStatus.Killed)
            {
                exitCode = shellArgs.MaxIdleKillIsAnError ? -1 : 0;
            }

            runOutput.ExitCode = exitCode;
            runOutput.Command = shellArgs.Executable;
            runOutput.CommandOutput = output;
            runOutput.FullOutput = logOutput.ToString();
            runOutput.ErrorOutput = errorOutput.ToString();
            LogProcessData($"Process exited with code '{exitCode}'", logOutput);
            hasErrors |= (exitCode != 0);

            if (hasErrors && shellArgs.ThrowOnError)
            {
                throw new Exception(errorOutput.ToString());
            }

            runOutput.Succeeded = !hasErrors;
            return runOutput;
        }

        private static void LogProcessData(string data, StringBuilder output)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            Console.WriteLine(data); // Editor.log
            output.AppendLine(data);
        }
    }
}
