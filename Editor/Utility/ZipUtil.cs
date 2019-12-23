using System;
using System.IO;
using Ionic.Zip;
using UnityEngine.Assertions;

namespace SsitEngine.Editor
{
    public class ZipUtil
    {
        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <param name="location"></param>
        public static void Unzip( string zipFilePath, string location )
        {
            Directory.CreateDirectory(location);

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.ExtractAll(location, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="relatedStr"></param>
        /// <param name="files"></param>
        public static void Zip( string zipFileName, string relatedStr, params string[] files )
        {
            var path = Path.GetDirectoryName(zipFileName);

            Assert.IsFalse(string.IsNullOrEmpty(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var zip = new ZipFile())
            {
                foreach (var file in files)
                {
                    var reIndex = file.IndexOf(relatedStr, StringComparison.Ordinal);

                    if (reIndex < 0)
                    {
                        zip.AddFile(file, "");
                    }
                    else
                    {
                        zip.AddFile(file, file.Substring(reIndex, file.LastIndexOf('/') - reIndex));
                    }
                }
                zip.Save(zipFileName);
            }
        }
    }
}