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
        public GitApplication Git
        { get; private set; }

        public Path Root
        { get; private set; }

        public Path RepositoryPath
        { get; private set; }

        public string CurrentBranch
        {
            get { return Git.Execute(new SymbolicRef {Name="HEAD"}); }
            set { Git.Execute(new Checkout {Name=value}); }
        }

        public IEnumerable<string> Branches
        {
            get { return Git.Execute(new GetBranches()); }
        }

        public static Path FindTopLevelDirectory(string directory)
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
            Git = new GitApplication(toplevelDirectory);
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

            var taskStatus = Task.Factory.StartNew(() => Git.Execute(new Commands.Status()));
            var taskClean = Task.Factory.StartNew(() => Git.Execute(clean));

            return new Status(
                taskStatus.Result.Concat(taskClean.Result.Select(name => new KeyValuePair<FileStatus, string>(FileStatus.Ignored, name))),
                Git.WorkingDirectory
            );
        }

        public FileStatus GetStatus(string path)
        {
            var status = new Commands.Status
            {
                Targets = Enumerable.Repeat(path, 1)
            };
            var result = Git.Execute(status).SingleOrDefault();
            return result.Value != null ? result.Key : FileStatus.NotModified;
        }
    }
}
