using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public class Diff : Command<string>
    {
        public bool ViewInTool
        { get; set; }

        public string FilePath
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (ViewInTool)
                runner.Arguments("difftool", "--no-prompt");
            else
                runner.Arguments("diff");

            runner.Arguments(FilePath);
        }
    }
}