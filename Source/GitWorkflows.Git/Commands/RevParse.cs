using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public sealed class RevParse : Command<string>
    {
        public override void Setup(Runner runner)
        { runner.Arguments("rev-parse", "--show-toplevel"); }
    }
}