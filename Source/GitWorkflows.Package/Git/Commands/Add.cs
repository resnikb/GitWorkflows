using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    public sealed class Add : Command<string>
    {
        public enum AddTarget
        {
            AllChanges,
            OnlyTracked
        }

        public AddTarget Target
        { get; set; }

        public bool AllowIgnored
        { get; set; }

        public IEnumerable<string> Files
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (Files == null || !Files.Any())
                Files = new[]{"."};

            runner.Arguments("add");
            if (AllowIgnored)
                runner.Arguments("-f");

            switch (Target)
            {
                case AddTarget.AllChanges:
                    runner.Arguments("-A");
                    break;

                case AddTarget.OnlyTracked:
                    runner.Arguments("-u");
                    break;
            }

            runner.Arguments(Files.ToArray());
        }
    }
}