using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public sealed class Init : Command<string>
    {
        public override void Setup(Runner runner)
        {
            runner.Arguments("init");
        }
    }
}