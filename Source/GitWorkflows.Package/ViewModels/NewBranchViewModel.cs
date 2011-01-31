namespace GitWorkflows.Package.ViewModels
{
    class NewBranchViewModel : ViewModel
    {
        public string SourceName
        { get; private set; }

        public string NewBranchName
        { get; set; }

        public bool CheckoutAfterCreating
        { get; set; }

        public NewBranchViewModel(string sourceName)
        {
            SourceName = sourceName;
            CheckoutAfterCreating = true;
        }
    }
}