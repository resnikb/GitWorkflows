namespace GitWorkflows.Package.Git
{
    class Branch
    {
        public string Name
        { get; private set; }
        
        public Branch(string name)
        { Name = name; }    
    }
}