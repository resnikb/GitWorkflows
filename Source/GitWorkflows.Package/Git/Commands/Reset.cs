using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    public sealed class Reset : Command<string>
    {
        public enum Mode
        {
            CheckOnly,
            IndexOnly,
            IndexAndTree,
            IndexAndUnmodifiedFiles
        }

        public Mode ResetMode
        { get; set; }

        public bool Quiet
        { get; set; }

        public string CommitName
        { get; set; }

        public IEnumerable<string> Paths
        { get; set; }

        public Reset()
        {
            ResetMode = Mode.IndexOnly;
            Quiet = true;
        }

        public override void Setup(Runner runner)
        {
            runner.Arguments("reset");
            
            switch (ResetMode)
            {
                case Mode.CheckOnly:
                    runner.Arguments("--soft");
                    break;

                case Mode.IndexOnly:
                    runner.Arguments("--mixed");
                    break;

                case Mode.IndexAndTree:
                    runner.Arguments("--hard");
                    break;

                case Mode.IndexAndUnmodifiedFiles:
                    runner.Arguments("--keep");
                    break;
            }

            if (Quiet)
                runner.Arguments("-q");

            if (!string.IsNullOrWhiteSpace(CommitName))
                runner.Arguments(CommitName);

            runner.Arguments("--");

            if (Paths != null)
                runner.Arguments(Paths.ToArray());
        }
    }
}