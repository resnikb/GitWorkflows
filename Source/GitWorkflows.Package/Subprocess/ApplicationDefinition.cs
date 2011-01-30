namespace GitWorkflows.Package.Subprocess
{
    public class ApplicationDefinition
    {
        public string Command
        { get; private set; }

        public string WorkingDirectory
        { get; private set; }

        public ApplicationDefinition(string command, string workingDirectory)
        {
            Command = command;
            WorkingDirectory = workingDirectory;
        }

        public Runner CreateRunner()
        { return new Runner(this); }
    }
}