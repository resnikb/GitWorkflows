using System;
using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Package.Extensions;
using GitWorkflows.Package.Subprocess;

namespace GitWorkflows.Package.Git.Commands
{
    public sealed class Status : Command<IEnumerable<KeyValuePair<FileStatus, string>>>
    {
        public IEnumerable<string> Targets
        { get; set; }

        public override void Setup(Runner runner)
        {
            runner.Arguments("status", "--porcelain");

            if (Targets != null)
                runner.Arguments(Targets.ToArray());
        }

        protected override IEnumerable<KeyValuePair<FileStatus, string>> Parse(ApplicationDefinition app, string content)
        {
            foreach (var line in content.GetLines())
            {
                FileStatus fileStatus;
                string path;
                if ( TryParse(line, out fileStatus, out path) )
                    yield return new KeyValuePair<FileStatus, string>(fileStatus, path);
            }
        }

        private static bool TryParse(string line, out FileStatus fileStatus, out string path)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                fileStatus = FileStatus.Untracked;
                path = null;
                return false;
            }

            var parts = line.Split(new[]{' ', '\t'}, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                fileStatus = FileStatus.Untracked;
                path = null;
                return false;
            }

            path = parts[1];
            switch (parts[0].ToUpperInvariant()[0])
            {
                case 'A':
                    fileStatus = FileStatus.Added;
                    break;

                case 'D':
                    fileStatus = FileStatus.Removed;
                    break;

                case 'C':
                    fileStatus = FileStatus.Copied;
                    break;

                case 'R':
                    fileStatus = FileStatus.Renamed;
                    break;

                case 'M':
                    fileStatus = FileStatus.Modified;
                    break;

                case 'U':
                    fileStatus = FileStatus.Conflicted;
                    break;

                case '=':
                    fileStatus = FileStatus.NotModified;
                    break;

                default:
                    fileStatus = FileStatus.Untracked;
                    break;
            }

            return true;
        }
    }
}