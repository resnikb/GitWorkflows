using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    public sealed class Init : Command<string>
    {
        public override void Setup(Runner runner)
        {
            runner.Arguments("init");
        }
    }
}