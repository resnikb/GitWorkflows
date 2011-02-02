using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitWorkflows.Common;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public class Checkout : Command<string>
    {
        private static readonly Tuple<Regex, Action<ApplicationDefinition, Tuple<int, string>>>[] _errorHandlerMap = new[]
        {
            new Tuple<Regex, Action<ApplicationDefinition, Tuple<int, string>>>(new Regex("Your local changes to the following files would be overwritten by checkout", RegexOptions.IgnoreCase), HandleUncomittedChanges),                                                                                          
        };

        public string Name
        { get; set; }

        public bool Force
        { get; set; }

        public bool CreateBranch
        { get; set; }

        public override void Setup(Runner runner)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Name must be specified");

            runner.Arguments("checkout");

            if (Force)
                runner.Arguments("-f");

            if (CreateBranch)
                runner.Arguments("-b");

            runner.Arguments(Name);
        }

        protected override void ThrowException(ApplicationDefinition app, Tuple<int, string> result)
        {
            var handler = _errorHandlerMap.FirstOrDefault(h => h.Item1.IsMatch(result.Item2));
            if (handler != null)
                handler.Item2(app, result);

            base.ThrowException(app, result);
        }

        private static void HandleUncomittedChanges(ApplicationDefinition app, Tuple<int, string> result)
        {
            var rootPath = new Path(app.WorkingDirectory);
            var modifiedFiles = result.Item2.GetLines()
                .SkipWhile(line => line.Length == 0 || !char.IsWhiteSpace(line[0]))
                .TakeWhile(line => line.Length > 0 && char.IsWhiteSpace(line[0]))
                .Select(line => rootPath.Combine(line.Trim()));
                                    
            throw new UncommittedChangesException(modifiedFiles);
        }
    }

    class UncommittedChangesException : Exception
    {
        public IEnumerable<Path> ModifiedFiles 
        { get; private set; }

        public UncommittedChangesException(IEnumerable<Path> modifiedFiles)
            : base("Your local changes would be overwritten by checkout")
        { ModifiedFiles = modifiedFiles.ToArray(); }
    }
}