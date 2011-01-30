using GitWorkflows.Package.Git.Commands;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git
{
    public class GitApplication : ApplicationDefinition
    {
        public GitApplication(string workingDirectory) : base(@"c:\Program Files (x86)\Git\bin\git.exe", workingDirectory)
        {}

        public T Execute<T>(Command<T> command)
        {
            var runner = CreateRunner();
            command.Setup(runner);

            var result = runner.Execute();
            return command.ProcessResult(this, result);
        }
    }
}