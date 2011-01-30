using System;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    class SymbolicRef : Command<string>
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