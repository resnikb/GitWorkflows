using GitWorkflows.Common.Subprocess;
using GitWorkflows.Git.Commands;

namespace GitWorkflows.Git
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

        public void ExecuteAsync<T>(Command<T> command)
        {
            var runner = CreateRunner();
            command.Setup(runner);

            runner.ExecuteAsync();
        }
    }
}