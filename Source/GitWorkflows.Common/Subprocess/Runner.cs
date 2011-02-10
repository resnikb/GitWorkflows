using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NLog;

namespace GitWorkflows.Common.Subprocess
{
    public sealed class Runner
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(Runner).FullName);

        private readonly List<string> _arguments = new List<string>();
        private readonly ApplicationDefinition _application;

        public Runner(ApplicationDefinition app)
        { _application = app; }

        public void Arguments(params string[] arguments)
        { _arguments.AddRange(arguments.Select(Quote)); }

        public void ExecuteAsync()
        { Execute(false); }

        public Tuple<int, string> Execute()
        { return Execute(true); }

        private Tuple<int, string> Execute(bool waitForProcess)
        {
            var output = new StringBuilder();
            var errors = new StringBuilder();

            var info = new ProcessStartInfo(_application.Command, _arguments.ToDelimitedString(" "))
            {
                WorkingDirectory = _application.WorkingDirectory ?? System.IO.Path.GetDirectoryName(_application.Command),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            Log.Info("** (in {0}) {1} {2}", info.WorkingDirectory, _application.Command, _arguments.ToDelimitedString(" "));
            var process = new Process();
            try
            {
                process.StartInfo = info;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += (sender, args) => AppendLine(errors, args);
                process.OutputDataReceived += (sender, args) => AppendLine(output, args);

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                if (!waitForProcess)
                    return null;

                process.WaitForExit();

                var outputBuffer = output;
                if (process.ExitCode != 0)
                    outputBuffer = errors;

                return new Tuple<int, string>(process.ExitCode, outputBuffer.ToString().Trim());
            }
            finally
            {
                if (waitForProcess)
                    process.Dispose();
            }
        }

        private static void AppendLine(StringBuilder output, DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                output.AppendLine(args.Data);
                Log.Info(":  " + args.Data);
            }
        }

        private static string Quote(string arg)
        { return arg.Any(char.IsWhiteSpace) ? string.Concat('"', arg, '"') : arg; }
    }
}