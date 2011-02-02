using System;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public class Branch : Command<string>
    {
        public string Name
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException(string.Format("Name not specified for new branch"));

            runner.Arguments("branch", Name);
        }
    }
}