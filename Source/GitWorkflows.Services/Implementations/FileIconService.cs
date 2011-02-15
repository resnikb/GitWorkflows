using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GitWorkflows.Common;

namespace GitWorkflows.Services.Implementations
{
    [Export(typeof(IFileIconService))]
    public class FileIconService : IFileIconService
    {
        public ImageSource GetIcon(string path)
        {
            return CreateIcon(path);
        }

        private static ImageSource CreateIcon(string path)
        {
            var shinfo = new PInvoke.SHFILEINFO();
            PInvoke.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), PInvoke.SHGFI_ICON | PInvoke.SHGFI_SMALLICON);

            var iconHandle = shinfo.hIcon;
            if (IntPtr.Zero == iconHandle)
                return null;

            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    iconHandle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                PInvoke.DestroyIcon(iconHandle);
            }
        }
    }
}