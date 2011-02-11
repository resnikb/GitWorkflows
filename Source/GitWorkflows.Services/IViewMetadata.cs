namespace GitWorkflows.Services
{
    /// <summary>
    /// Interface representing the metadata exposed by a view.
    /// </summary>
    public interface IViewMetadata
    {
        /// <summary>
        /// Gets the window title.
        /// </summary>
        /// 
        /// <value>The window title.</value>
        string WindowTitle
        { get; }    
    }
}