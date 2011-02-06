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

            var path = parts[1];

            switch (parts[0].ToUpperInvariant()[0])
            {
                case 'A':
                    AddStatus(baseDirectory, result, path, FileStatus.Added);
                    break;

                case 'D':
                    AddStatus(baseDirectory, result, path, FileStatus.Removed);
                    break;

                case 'C':
                    AddStatus(baseDirectory, result, path, FileStatus.CopySource, FileStatus.CopyDestination);
                    break;

                case 'R':
                    AddStatus(baseDirectory, result, path, FileStatus.RenameSource, FileStatus.RenameDestination);
                    break;

                case 'M':
                    AddStatus(baseDirectory, result, path, FileStatus.Modified);
                    break;

                case 'U':
                    AddStatus(baseDirectory, result, path, FileStatus.Conflicted);
                    break;

                case '=':
                    AddStatus(baseDirectory, result, path, FileStatus.NotModified);
                    break;

                default:
                    AddStatus(baseDirectory, result, path, FileStatus.Untracked);
                    break;
            }
        }

        private static void AddStatus(string baseDirectory, List<Git.Status> result, string pathSpecification, FileStatus sourceStatus, FileStatus? destinationStatus = null)
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
                result.Add(new Git.Status(System.IO.Path.Combine(baseDirectory, destination), destinationStatus.Value, sourceStatusObject));
            }
        }
    }
}