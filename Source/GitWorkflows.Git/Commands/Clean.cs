using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public sealed class Clean : Command<IEnumerable<string>>
    {
        public enum CleanTarget
        {
            Ignored,
            UntrackedButNotIgnored,
            AllUntracked
        };

        public bool DryRun
        { get; set; }

        public CleanTarget Target
        { get; set; }

        public bool IncludeDirectories
        { get; set; }

        public Clean()
        {
            Target = CleanTarget.UntrackedButNotIgnored;
            DryRun = true;
            IncludeDirectories = true;
        }

        public override void Setup(Runner runner)
        {
            runner.Arguments("clean", DryRun ? "-n" : "-f");
            if (IncludeDirectories)
                runner.Arguments("-d");
            
            switch (Target)
            {
                case CleanTarget.AllUntracked:
                    runner.Arguments("-x");
                    break;

                case CleanTarget.Ignored:
                    runner.Arguments("-X");
                    break;

                case CleanTarget.UntrackedButNotIgnored:
                    break;
            }
        }

        protected override IEnumerable<string> Parse(ApplicationDefinition app, string output)
        {
            var separator = new[]{' '};
            return output.GetLines()
                         .Select(line => line.Split(separator, 3))
                         .Where(words => words.Length == 3 && words[0] == "Would" && words[1] == "remove")
                         .Select(words => words[2]);
        }
    }
}