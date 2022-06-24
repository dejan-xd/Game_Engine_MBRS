using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Editor.Common
{
    static class VisualExtensions
    {
        public static T FindVisualParent<T>(this DependencyObject depObject) where T : DependencyObject
        {
            if (depObject is not Visual) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(depObject);
            while (parent != null)
            {
                if (parent is T type)
                {
                    return type;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }

    public static class ContentHelper
    {
        public static string GetRandomString(int length = 8)
        {
            if (length <= 0) length = 8;
            var n = length / 11;
            StringBuilder sb = new();
            for (int i = 0; i <= n; ++i)
            {
                sb.Append(Path.GetRandomFileName().Replace(".", ""));
            }

            return sb.ToString(0, length);
        }

        public static bool IsDirectory(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return false;
        }

        public static bool IsOlder(this DateTime date, DateTime other) => date < other;

        public static string SanitizeFileName(string name)
        {
            StringBuilder path = new(name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            StringBuilder file = new(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);

            foreach (char c in Path.GetInvalidPathChars())
            {
                path.Replace(c, '_');
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                file.Replace(c, '_');
            }

            return path.Append(file).ToString();
        }

        public static byte[] ComputeHash(byte[] data, int offset = 0, int count = 0)
        {
            if (data?.Length > 0)
            {
                using SHA256 sha256 = SHA256.Create();
                return sha256.ComputeHash(data, offset, count > 0 ? count : data.Length);
            }
            return null;
        }
    }
}
