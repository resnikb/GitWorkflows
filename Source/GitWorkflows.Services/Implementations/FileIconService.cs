using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<Path, ImageSource> _cachedFileIcons = new ConcurrentDictionary<Path, ImageSource>();
        private readonly ConcurrentDictionary<string, ImageSource> _cachedExtensionIcons = new ConcurrentDictionary<string, ImageSource>();

        public ImageSource GetIcon(Path path)
        { return _cachedFileIcons.GetOrAdd(path, CreateFileIcon); }

        private ImageSource CreateFileIcon(Path path)
        {
            var extension = path.HasExtension ? path.Extension.ToLowerInvariant() : null;
            if (extension == null || extension == ".exe" || extension == ".dll")
                return CreateIcon(path);

            return _cachedExtensionIcons.GetOrAdd(extension, _ => CreateIcon(path));
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