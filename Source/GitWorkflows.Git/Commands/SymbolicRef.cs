using System;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public class SymbolicRef : Command<string>
    {
        public string Name
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Name of symbolic reference must be specified");

            runner.Arguments("symbolic-ref", Name);
        }

        protected override string Parse(ApplicationDefinition app, string contents)
        { return System.IO.Path.GetFileName(base.Parse(app, contents)); }
    }
}