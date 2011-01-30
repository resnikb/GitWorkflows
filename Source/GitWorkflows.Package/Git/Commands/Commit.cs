using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    public sealed class Commit : Command<string>
    {
        public bool AutoStage
        { get; set; }

        public string Message
        { get; set; }

        public override void Setup(Runner runner)
        {
            runner.Arguments("commit");

            if (AutoStage)
                runner.Arguments("-a");

            if (!string.IsNullOrEmpty(Message))
                runner.Arguments("-m", Message);
        }
    }
}