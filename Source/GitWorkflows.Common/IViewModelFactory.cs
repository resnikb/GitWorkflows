namespace GitWorkflows.Common
{
    /// <summary>
    /// Represents a factory that creates view models.
    /// </summary>
    public interface IViewModelFactory
    {
        /// <summary>
        /// Creates a view model optionally passing some arguments to the constructor.
        /// </summary>
        /// 
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// 
        /// <param name="constructorArgs">The constructor arguments.</param>
        /// 
        /// <returns>Created view model</returns>
        TViewModel Create<TViewModel>(params object[] constructorArgs);
    }
}