using System.Collections.Generic;
using System.IO;

namespace GitWorkflows.Common
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Enumerate all lines in the given string.
        /// </summary>
        /// 
        /// <param name="input">The input string. Can be <c>null</c>.</param>
        /// 
        /// <returns>Lazy collection of lines in the input string. The collection is empty if input
        /// is <c>null</c> or empty.</returns>
        public static IEnumerable<string> GetLines(this string input)
        {
            if (string.IsNullOrEmpty(input))
                yield break;

            using (var reader = new StringReader(input))
            {
                string line;
                while ( (line = reader.ReadLine()) != null )
                    yield return line;
            }
        }
    }
}