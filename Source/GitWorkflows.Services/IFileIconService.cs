using System.Windows.Media;
using GitWorkflows.Common;

namespace GitWorkflows.Services
{
    /// <summary>
    /// Represents a service that provides icons for files.
    /// </summary>
    public interface IFileIconService
    {
        /// <summary>
        /// Gets the icon for the given file.
        /// </summary>
        /// 
        /// <param name="path">The path to the file.</param>
        /// 
        /// <returns>Image for the given file.</returns>
        ImageSource GetIcon(Path path);
    }
}