using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GitWorkflows.Package.FileSystem
{
    [DebuggerDisplay("{ActualPath}")]
    public class Path : IEquatable<Path>, IEquatable<string>, IComparable<Path>, IComparable<string>, IComparable
    {
        private readonly Lazy<string> _actualPath;
        private readonly Lazy<string> _canonicalPath;

        public string ActualPath
        {
            get { return _actualPath.Value; }
        }

        public string CanonicalPath
        {
            get { return _canonicalPath.Value; }
        }

        public string DirectoryName
        {
            get { return System.IO.Path.GetDirectoryName(ActualPath); }
        }

        public string FileName
        {
            get { return System.IO.Path.GetFileName(ActualPath); }
        }

        public string Extension
        {
            get { return System.IO.Path.GetExtension(ActualPath); }
        }

        public string FileNameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(ActualPath); }
        }

        public bool HasExtension
        {
            get { return System.IO.Path.HasExtension(ActualPath); }
        }

        public bool IsRelative
        {
            get { return !System.IO.Path.IsPathRooted(ActualPath); }
        }

        public bool IsDirectory
        {
            get { return Directory.Exists(ActualPath); }
        }

        public bool IsFile
        {
            get { return File.Exists(ActualPath); }
        }

        public bool Exists
        {
            get { return IsDirectory || IsFile; }
        }

        public FileSystemInfo Info
        {
            get { return IsDirectory ? (FileSystemInfo)new DirectoryInfo(ActualPath) : new FileInfo(ActualPath); }
        }

        public Path(string relativeOrAbsolute)
        {
            if (relativeOrAbsolute == null)
                throw new ArgumentNullException("relativeOrAbsolute");

            if (relativeOrAbsolute.Length == 0)
                throw new ArgumentException("Path is empty", "relativeOrAbsolute");

            if (relativeOrAbsolute.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("Path contains invalid characters", "relativeOrAbsolute");

            _canonicalPath = new Lazy<string>(() => ActualPath.ToLowerInvariant(), false);
            _actualPath = new Lazy<string>(() => GetActualPath(relativeOrAbsolute), false);
        }

        public Path Combine(string p1)
        { return new Path(System.IO.Path.Combine(ActualPath, p1)); }

        public Path Combine(string p1, string p2)
        { return new Path(System.IO.Path.Combine(ActualPath, p1, p2)); }

        public Path Combine(string p1, string p2, string p3)
        { return new Path(System.IO.Path.Combine(ActualPath, p1, p2, p3)); }

        public Path Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return this;

            var allPaths = new string[paths.Length+1];
            allPaths[0] = ActualPath;
            Array.Copy(paths, 0, allPaths, 1, paths.Length);

            return new Path(System.IO.Path.Combine(allPaths));
        }

        public Path ChangeExtension(string extension)
        { return new Path(System.IO.Path.ChangeExtension(ActualPath, extension)); }

        public Path GetRelativeTo(Path other)
        {
            var components = CanonicalPath.Split(System.IO.Path.DirectorySeparatorChar);
            var otherComponents = other.CanonicalPath.Split(System.IO.Path.DirectorySeparatorChar);

            int i = 0;
            while (i < components.Length && i < otherComponents.Length && components[i] == otherComponents[i])
                ++i;

            var path = string.Join(
                System.IO.Path.DirectorySeparatorChar.ToString(), 
                Enumerable.Repeat("..", otherComponents.Length-i).Concat(components.Skip(i))
            );
            return new Path(path.Length == 0 ? "." : path);
        }

        public Path GetCommonPrefix(Path other)
        {
            var components = GetCanonicalComponents();
            var otherComponents = other.GetCanonicalComponents();

            int i = 0;
            while (i < components.Length && i < otherComponents.Length && components[i] == otherComponents[i])
                ++i;

            var path = string.Join(
                System.IO.Path.DirectorySeparatorChar.ToString(), 
                components.Take(i)
            );
            return path.Length > 0 ? new Path(path) : null;
        }

        public string[] GetCanonicalComponents()
        { return CanonicalPath.Split(System.IO.Path.DirectorySeparatorChar); }

        public bool IsParentOf(Path path)
        { return GetCommonPrefix(path) == this; }

        public bool Equals(Path other)
        { return !ReferenceEquals(other, null) && other.CanonicalPath == CanonicalPath; }

        public bool Equals(string other)
        { return !ReferenceEquals(other, null) && CanonicalPath == GetCanonicalPath(other); }

        public override bool Equals(object obj)
        { return Equals(obj as Path) || Equals(obj as string); }

        public int CompareTo(Path other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            return CanonicalPath.CompareTo(other.CanonicalPath);
        }

        public int CompareTo(string other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            return CanonicalPath.CompareTo(GetCanonicalPath(other));
        }

        int IComparable.CompareTo(object other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            var otherAsPath = other as Path;
            if (!ReferenceEquals(otherAsPath, null))
                return CompareTo(otherAsPath);

            var otherAsString = other as string;
            if (!ReferenceEquals(otherAsString, null))
                return CompareTo(otherAsString);

            throw new ArgumentException("Cannot compare path to object of type "+other.GetType(), "other");
        }

        public override int GetHashCode()
        { return CanonicalPath.GetHashCode(); }

        public override string ToString()
        { return ActualPath; }

        public static bool operator == (Path lhs, Path rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator == (Path lhs, string rhs)
        {
            if (ReferenceEquals(lhs, null))
                return ReferenceEquals(rhs, null);

            return lhs.Equals(rhs);
        }

        public static bool operator == (string lhs, Path rhs)
        { return rhs == lhs; }

        public static bool operator !=(string lhs, Path rhs)
        { return !(lhs == rhs); }

        public static bool operator !=(Path lhs, string rhs)
        { return !(lhs == rhs); }

        public static bool operator !=(Path lhs, Path rhs)
        { return !(lhs == rhs); }

        public static implicit operator Path(string relativeOrAbsolute)
        { return new Path(relativeOrAbsolute); }

        public static implicit operator string(Path path)
        { return path.ActualPath; }

        private static string GetNormalizedPath(string relativeOrAbsolute)
        {
            // Split into path components
            var components = relativeOrAbsolute.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            var stack = new Stack<string>(components.Length);
            foreach (var component in components)
            {
                if (component == "..")
                {
                    if (stack.Count > 0 && stack.Peek() != "..")
                    {
                        // If this is a drive letter, just ignore the parent directory
                        if (stack.Peek().Last() != ':')
                            stack.Pop();
                    }
                    else
                        stack.Push(component);
                }
                else if (!string.IsNullOrEmpty(component) && component != ".")
                    stack.Push(component);
            }

            var path = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), stack.Reverse());
            if (path.Length > 0 && path.Last() == ':')
                path += System.IO.Path.DirectorySeparatorChar;

            return path.Length > 0 ? path : ".";
        }

        private static string GetActualPath(string relativeOrAbsolute)
        {
            var normalizedPath = GetNormalizedPath(relativeOrAbsolute);

            // If the path is not an absolute one, then just return it
            if (!System.IO.Path.IsPathRooted(normalizedPath))
                return normalizedPath;

            // If the path does not exist, there is not much we can do
            if (!File.Exists(normalizedPath) && !Directory.Exists(normalizedPath))
                return normalizedPath;

            var sb = new StringBuilder(Math.Max(1024, 2*normalizedPath.Length));
            var shortNameLength = GetShortPathName(normalizedPath, sb, (uint)sb.Capacity);
            var longNameLength = GetLongPathName(sb.ToString(0, (int)shortNameLength), sb, (uint)sb.Capacity);
            return sb.ToString(0, (int)longNameLength);
        }

        private static string GetCanonicalPath(string relativeOrAbsolute)
        { return GetActualPath(relativeOrAbsolute).ToLowerInvariant(); }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError=true)]
        static extern uint GetShortPathName(
           [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
           [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
           uint cchBuffer
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetLongPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszShortPath,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszLongPath,
            uint cchBuffer
        );
    }
}
