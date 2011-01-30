using System.Collections.Generic;
using System.IO;

namespace GitWorkflows.Package.Extensions
{
    public static class StringExtensions
    {
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