using System;
using System.IO;

namespace SsitEngine
{
    /// <summary>
    ///     文件相关工具函数
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        ///     获取文件名
        /// </summary>
        /// <returns>文件名.</returns>
        /// <param name="filename">文件的完整路径.</param>
        public static string GetShortFilename( string filename )
        {
            var indexOfBackslash = filename.LastIndexOf("\\");
            if (indexOfBackslash >= 0)
            {
                return filename.Substring(indexOfBackslash + 1);
            }

            var indexOfSlash = filename.LastIndexOf("/");
            if (indexOfSlash >= 0)
            {
                return filename.Substring(indexOfSlash + 1);
            }

            return filename;
        }

        /// <summary>
        ///     获取文件所在文件夹路径.
        /// </summary>
        /// <returns>文件夹名称.</returns>
        /// <param name="filename">文件的完整路径.</param>
        public static string GetFileDirectory( string filename )
        {
            var indexOfBackslash = filename.LastIndexOf("\\");
            if (indexOfBackslash >= 0)
            {
                return filename.Substring(0, indexOfBackslash);
            }

            var indexOfSlash = filename.LastIndexOf("/");
            if (indexOfSlash >= 0)
            {
                return filename.Substring(0, indexOfSlash);
            }

            return null;
        }

        /// <summary>
        ///     获取不带扩展的文件名称
        /// </summary>
        /// <returns>The filename without extension.</returns>
        /// <param name="filename">Full filename.</param>
        public static string GetFilenameWithoutExtension( string filename )
        {
            var indexOfDot = filename.LastIndexOf('.');
            if (indexOfDot < 0)
            {
                return null;
            }

            var indexOfBackslash = filename.LastIndexOf("\\");
            if (indexOfBackslash >= 0)
            {
                return filename.Substring(indexOfBackslash + 1, indexOfDot - indexOfBackslash - 1);
            }

            var indexOfSlash = filename.LastIndexOf("/");
            if (indexOfSlash >= 0)
            {
                return filename.Substring(indexOfSlash + 1, indexOfDot - indexOfSlash - 1);
            }

            return null;
        }

        /// <summary>
        ///     获取文件的扩展名称
        /// </summary>
        /// <returns>文件的扩展名称.</returns>
        /// <param name="filename">文件的完整路径  data.txt.--> .txt</param>
        public static string GetFileExtension( string filename )
        {
            var lastDot = filename.LastIndexOf('.');
            if (lastDot < 0)
            {
                return null;
            }

            return filename.Substring(lastDot).ToLowerInvariant();
        }

        /// <summary>
        ///     根据文件路径获取文件名称.
        /// </summary>
        /// <returns>文件名称（带扩展）.</returns>
        /// <param name="path">文件路径.</param>
        public static string GetFilename( string path )
        {
            var filename = Path.GetFileName(path);
            if (path == filename)
            {
                var indexOfBackslash = path.LastIndexOf("\\");
                if (indexOfBackslash >= 0)
                {
                    return path.Substring(indexOfBackslash + 1);
                }

                var indexOfSlash = path.LastIndexOf("/");
                if (indexOfSlash >= 0)
                {
                    return path.Substring(indexOfSlash + 1);
                }

                return path;
            }

            return filename;
        }

        /// <summary>
        ///     读取文件径路的文件.
        /// </summary>
        /// <returns>字节数组.</returns>
        /// <param name="filename">文件路径.</param>
        public static byte[] LoadFileData( string filename )
        {
            try
            {
                if (filename == null)
                {
                    return new byte[0];
                }

                return File.ReadAllBytes(filename.Replace('\\', '/'));
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
    }
}