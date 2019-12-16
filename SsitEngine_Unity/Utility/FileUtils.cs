/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：依托Unity底层的文件工具类                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/29                             
*└──────────────────────────────────────────────────────────────┘
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SsitEngine.DebugLog;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace SsitEngine.Unity
{
    /// <summary>
    /// 文件工具类
    /// </summary>
    public static class FileUtils
    {
        public static bool IsExistFile( string path )
        {
            return File.Exists(path);
        }

        public static string GetFileNameFromPath( string path )
        {
            var index = path.LastIndexOf(@"\");
            index = index > 0 ? index + 1 : 0;

            return path.Substring(index);
        }

        public static string GetFileNameFromFixedPath( string path )
        {
            var index = path.LastIndexOf(@"/");
            index = index > 0 ? index + 1 : 0;

            return path.Substring(index);
        }

        public static string CreateFile( string name, byte[] data, int length, string savePath = "" )
        {
            if (string.IsNullOrEmpty(name))
            {
                SsitDebug.Error("下载文件相对路径异常，请联系服务器开发人员");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(savePath)) savePath = Application.persistentDataPath;

            var filePath = savePath + "/" + name;


            var path = PathUtils.GetPathWithoutFile(filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var fsStream = new FileStream(filePath, FileMode.Create))
            {
                fsStream.Write(data, 0, length);
                //将文件写出到硬盘（防止中断丢失写入文件）
                fsStream.Flush();
            }
            return filePath;
        }

        /// <summary>
        /// 获取对应文件名称为uuid的文件
        /// </summary>
        /// <param name="uuid">文件uuid</param>
        /// <param name="folderPath">文件夹</param>
        /// <returns></returns>
        public static string GetFileWithOutExtension( string uuid, string folderPath )
        {
            if (Directory.Exists(folderPath))
            {
                var searchResult = Directory.GetFiles(folderPath, $"{uuid}.*");
                if (searchResult.Length == 0) return null;

                if (searchResult.Length != 1) SsitDebug.Warning("you search folder is not simple one");
                return searchResult[0];
            }

            return null;
        }

        /// <summary>
        /// 构建文件的MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string BuildMD5( string filePath )
        {
            var fs = File.OpenRead(filePath);

            var md5 = MD5.Create();

            var fileMd5Bytes = md5.ComputeHash(fs);

            fs.Close();
            fs.Dispose();
            return BitConverter.ToString(fileMd5Bytes).Replace("-", "").ToLower();
        }

        #region Folder

        public static void CopyFolder( string srcPath, string destPath )
        {
            //如果源文件夹不存在，则创建
            if (!Directory.Exists(srcPath)) Directory.CreateDirectory(srcPath);

            //如果目标文件夹中没有源文件夹则在目标文件夹中创建源文件夹
            if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);

            var dir = new DirectoryInfo(srcPath);

            var filesInfo = dir.GetFileSystemInfos();


            for (var i = 0; i < filesInfo.Length; i++)
            {
                var tmpFile = filesInfo[i];
                var tmpSoure = srcPath + "\\" + tmpFile.Name;
                tmpSoure = PathUtils.FixedPath(tmpSoure);


                var tmpDist = destPath + "\\" + tmpFile.Name;

                tmpDist = PathUtils.FixedPath(tmpDist);


                if (tmpFile is FileInfo)
                    File.Copy(tmpSoure, tmpDist, true);
                else if (tmpFile is DirectoryInfo) CopyFolder(tmpSoure, tmpDist);
            }
        }

        /// <summary>
        /// 复制streaming路径到Presitent
        /// </summary>
        /// <param name="streamPath"></param>
        /// <param name="onFinishedAction"></param>
        public static void CopyFolderFormStreamToPersitentPath( string streamPath, Action onFinishedAction = null )
        {
            Assert.IsFalse(string.IsNullOrEmpty(streamPath));
            string sP = Path.Combine(Application.streamingAssetsPath, streamPath);
            string pP = Path.Combine(Application.persistentDataPath, streamPath);
            if (!File.Exists(pP))
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                //using (UnityWebRequest www = UnityWebRequest.Get(sP))
#pragma warning disable 618
                using (WWW www = new WWW(sP))
#pragma warning restore 618
                {
                    while (!www.isDone)
                    {
                    }

                    if (www.isDone && string.IsNullOrEmpty(www.error))
                    {
                        string folderPath = Path.GetDirectoryName(pP);
                        Assert.IsFalse(string.IsNullOrEmpty(folderPath), "存放路径不存在");
                        if (!Directory.Exists(folderPath))
                        {
                            //创建文件夹
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllBytes(pP, www.bytes);
                        //File.WriteAllBytes(pP, www.downloadHandler.data);
                        onFinishedAction?.Invoke();
                    }
                    else
                    {
                        Debug.Log(www.error);
                    }
                }
#else
                string folderPath = Path.GetDirectoryName(pP);
                Assert.IsFalse(string.IsNullOrEmpty(folderPath), "存放路径不存在");
                if (!Directory.Exists(folderPath))
                {
                    //创建文件夹
                    Directory.CreateDirectory(folderPath);
                }
                File.Copy(sP, pP,true);
                onFinishedAction?.Invoke();
#endif
            }
            else
            {
                onFinishedAction?.Invoke();
            }
        }

        /// <summary>
        /// 复制多个streaming路径到Presitent
        /// </summary>
        /// <param name="streamPaths"></param>
        /// <param name="onFinishedAction"></param>
        public static void CopyFolderFormStreamToPersitentPath( List<string> streamPaths,
            Action onFinishedAction = null )
        {
            int copyCount = streamPaths.Count;

            int curCopyCount = 0;

            void OnListener()
            {
                curCopyCount++;
            }

            for (int i = 0; i < streamPaths.Count; i++)
            {
                CopyFolderFormStreamToPersitentPath(streamPaths[i], OnListener);
            }

            Assert.IsFalse(curCopyCount != copyCount, "拷贝异常");

            onFinishedAction?.Invoke();
        }

        public static void DeleteFolder( string dir )
        {
            foreach (var d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    var fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly", StringComparison.Ordinal) != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);
                }
                else
                {
                    DeleteFolder(d);
                }

                if (Directory.Exists(d)) Directory.Delete(d);
            }

            if (Directory.Exists(dir)) Directory.Delete(dir);
        }

        #endregion

        #region MemCpy

        public static void MemCpy( byte[] des, byte[] source, int desStart )
        {
            if (des == null || source == null) return;

            Buffer.BlockCopy(source, 0, des, desStart, source.Length);
        }

        public static void MemCpy( byte[] des, int desStart, byte[] sou, int souStart, int length )
        {
            if (des == null || sou == null) return;

            Buffer.BlockCopy(sou, souStart, des, desStart, length);
        }

        public static void MemCpy( byte[] des, byte[] sou, int desStart, int length )
        {
            Buffer.BlockCopy(sou, 0, des, desStart, length);
        }

        public static void MemCpy( byte[] des, byte[] sou, int desStart, int souStart, int length )
        {
            if (des.Length - desStart < length || sou.Length - souStart < length) return;

            Buffer.BlockCopy(sou, souStart, des, desStart, length);
        }

        #endregion
    }
}