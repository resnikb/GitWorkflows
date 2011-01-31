using System;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    class Branch : Command<string>
    {
        public string Name
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException(string.Format("Name not specified for new branch"));

            runner.Arguments(Name);
        }
    }
}