/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：压缩包工具(PC 安卓)                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/05/06                        
*└──────────────────────────────────────────────────────────────┘
*/


using System.IO;
using Ionic.Zip;
using UnityEngine;

namespace SsitEngine.Unity
{
    public class ZipUtil
    {
//#if UNITY_IPHONE
//        [DllImport("__Internal")]
//	private static extern void unzip (string zipFilePath, string location);
//
//	[DllImport("__Internal")]
//	private static extern void zip (string zipFilePath);
//
//	[DllImport("__Internal")]
//	private static extern void addZipFile (string addFile);
//
//#endif
        public static void Unzip( string zipFilePath, string location )
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                {
                    Directory.CreateDirectory(location);

                    using (var zip = ZipFile.Read(zipFilePath))
                    {
                        zip.ExtractAll(location, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                    break;
                case RuntimePlatform.Android:
                {
                    using (var zipper = new AndroidJavaClass("com.tsw.zipper"))
                    {
                        zipper.CallStatic("unzip", zipFilePath, location);
                    }
                }
                    break;
            }

//#if UNITY_IPHONE
//            unzip(zipFilePath, location);
//#endif
        }

        public static void Zip( string zipFileName, params string[] files )
        {
            var path = Path.GetDirectoryName(zipFileName);
            Directory.CreateDirectory(path);
            using (var zip = new ZipFile())
            {
                foreach (var file in files)
                    if (file.IndexOf("AssetBundles") < 0)
                        zip.AddFile(file, "");
                    else
                        zip.AddFile(file,
                            file.Substring(file.IndexOf("AssetBundles"),
                                file.LastIndexOf('/') - file.IndexOf("AssetBundles")));
                zip.Save(zipFileName);
            }

//#if UNITY_ANDROID
//            using (AndroidJavaClass zipper = new AndroidJavaClass("com.tsw.zipper"))
//            {
//                {
//                    zipper.CallStatic("zip", zipFileName, files);
//                }
//            }
//#elif UNITY_IPHONE
//            foreach (string file in files)
//            {
//                addZipFile(file);
//            }
//            zip(zipFileName);
//#endif
        }
    }
}