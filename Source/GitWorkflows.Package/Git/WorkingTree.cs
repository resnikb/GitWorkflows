using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitWorkflows.Package.FileSystem;
using GitWorkflows.Package.Git.Commands;

namespace GitWorkflows.Package.Git
{
    public class WorkingTree
    {
        private readonly GitApplication _git;

        public Path Root
        { get; private set; }

        public Path RepositoryPath
        { get; private set; }

        public string CurrentBranch
        {
            get { return _git.Execute(new SymbolicRef {Name="HEAD"}); }
            set { _git.Execute(new Checkout {Name=value}); }
        }

        public IEnumerable<string> Branches
        {
            get { return _git.Execute(new GetBranches()); }
        }

        public static string FindTopLevelDirectory(string directory)
        {
            try
            {
                var revParse = new RevParse(RevParse.PropertyOption.TopLevelDirectory);
                return new GitApplication(directory).Execute(revParse);
            }
            catch
            {
                return null;
            }
        }

        public WorkingTree(string toplevelDirectory)
        {
            Debug.Assert(toplevelDirectory == new Path(FindTopLevelDirectory(toplevelDirectory)));
            _git = new GitApplication(toplevelDirectory);
            Root = toplevelDirectory;
            RepositoryPath = Root.Combine(".git");
        }

        public Status GetStatus()
        {
            var clean = new Clean
            {
                Target = Clean.CleanTarget.Ignored,
                IncludeDirectories = false
            };

            var taskStatus = Task.Factory.StartNew(() => _git.Execute(new Commands.Status()));
            var taskClean = Task.Factory.StartNew(() => _git.Execute(clean));

            return new Status(
                taskStatus.Result.Concat(taskClean.Result.Select(name => new KeyValuePair<FileStatus, string>(FileStatus.Ignored, name))),
                _git.WorkingDirectory
            );
        }

        public FileStatus GetStatus(string path)
        {
            var status = new Commands.Status
            {
                Targets = Enumerable.Repeat(path, 1)
            };
            var result = _git.Execute(status).SingleOrDefault();
            return result.Value != null ? result.Key : FileStatus.NotModified;
        }
    }
}
