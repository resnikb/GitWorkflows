using System;
using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public sealed class Status : Command<IEnumerable<Git.Status>>
    {
        public IEnumerable<string> Targets
        { get; set; }

        public override void Setup(Runner runner)
        {
            runner.Arguments("status", "--porcelain", "--untracked-files=all");

            if (Targets != null)
                runner.Arguments(Targets.ToArray());
        }

        protected override IEnumerable<Git.Status> Parse(ApplicationDefinition app, string content)
        {
            var result = new List<Git.Status>();
            content.GetLines().ForEach(line => TryParse(line, app.WorkingDirectory, result));
            return result;
        }

        private static void TryParse(string line, string baseDirectory, List<Git.Status> result )
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            var parts = line.Split(new[]{' ', '\t'}, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return;

            var pathSpecification = parts[1];
            var path = System.IO.Path.Combine(baseDirectory, pathSpecification);

            switch (parts[0].ToUpperInvariant()[0])
            {
                case 'A':
                    result.Add(new Git.Status(path, FileStatus.Added));
                    break;

                case 'D':
                    result.Add(new Git.Status(path, FileStatus.Removed));
                    break;

                case 'C':
                    AddTwoPaths(pathSpecification, FileStatus.CopySource, FileStatus.CopyDestination, baseDirectory, result);
                    break;

                case 'R':
                    AddTwoPaths(pathSpecification, FileStatus.RenameSource, FileStatus.RenameDestination, baseDirectory, result);
                    break;

                case 'M':
                    result.Add(new Git.Status(path, FileStatus.Modified));
                    break;

                case 'U':
                    result.Add(new Git.Status(path, FileStatus.Conflicted));
                    break;

                case '=':
                    result.Add(new Git.Status(path, FileStatus.NotModified));
                    break;

                default:
                    result.Add(new Git.Status(path, FileStatus.Untracked));
                    break;
            }
        }

        private static void AddTwoPaths(string pathSpecification, FileStatus sourceStatus, FileStatus destinationStatus, string baseDirectory, List<Git.Status> result)
        {
            var splitIndex = pathSpecification.IndexOf("->");
            if (splitIndex < 0)
                result.Add(new Git.Status(System.IO.Path.Combine(baseDirectory, pathSpecification), sourceStatus));
            else
            {
                var source = pathSpecification.Substring(0, splitIndex).Trim();
                var destination = pathSpecification.Substring(splitIndex+2).Trim();

                var sourceStatusObject = new Git.Status(System.IO.Path.Combine(baseDirectory, source), sourceStatus);
                result.Add(sourceStatusObject);
                result.Add(new Git.Status(System.IO.Path.Combine(baseDirectory, destination), destinationStatus, sourceStatusObject));
            }
        }
    }
}