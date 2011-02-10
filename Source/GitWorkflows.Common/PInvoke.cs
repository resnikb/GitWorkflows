using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GitWorkflows.Common
{
    public static class PInvoke
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetLongPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszShortPath,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszLongPath,
            uint cchBuffer
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError=true)]
        public static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
            uint cchBuffer
        );

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);
    }
}