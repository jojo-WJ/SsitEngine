using UnityEngine;

namespace SsitEngine.Unity
{
    public class PathUtils
    {
        public const string kAssetBundlesOutputPath = "AssetBundles";

        public static string FixedPath( string path )
        {
            return path.Replace("\\", "/");
        }

        public static string GetAssetBundleFilePath( string path )
        {
            var tmpPath = "/" + kAssetBundlesOutputPath + "/" + GetRuntimeFolder() + "/" + path;
            return Application.persistentDataPath + tmpPath;
            //            switch (Application.platform)
            //            {
            //                case RuntimePlatform.WindowsEditor:
            //                case RuntimePlatform.OSXEditor:
            //                    {
            //                        return Application.streamingAssetsPath + tmpPath;
            //                    }
            //                default:
            //                    {
            //                        return Application.persistentDataPath + tmpPath;
            //                    }
            //            }
        }

        public static string GetPathWithoutFileExtension( string filePath )
        {
            if (string.IsNullOrEmpty(filePath) || filePath.LastIndexOf('.') < 0)
            {
                return filePath;
            }

            return filePath.Substring(0, filePath.LastIndexOf('.'));
        }

        public static string GetPathWithoutFile( string filePath )
        {
            filePath = FixedPath(filePath);

            if (string.IsNullOrEmpty(filePath) || filePath.LastIndexOf('/') < 0)
            {
                return filePath;
            }

            return filePath.Substring(0, filePath.LastIndexOf('/'));
        }


        public static string GetRuntimeFolder()
        {
            var folderName = "UnKnown";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                {
                    folderName = "Android";
                }
                    break;
                case RuntimePlatform.IPhonePlayer:
                {
                    folderName = "IOS";
                }
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                {
                    folderName = "Windows";
                }
                    break;
                case RuntimePlatform.WebGLPlayer:
                {
                    folderName = "WebGL";
                }
                    break;
#if UNITY_5
                case RuntimePlatform.WindowsWebPlayer:
                    {
                        folderName = "WebPlayer";
                    }
                    break;
#endif
            }

            return folderName;
        }

        public static string GetRuntimeFolderWWWHead()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                {
                    return "file:///";
                }
                case RuntimePlatform.Android:
                {
                    return "jar:file://";
                }
                default:
                {
                    return "file://";
                }
            }
        }

        public static string GetStreamingPath()
        {
            return Application.streamingAssetsPath;
        }

        public static string GetPersistentDataPath()
        {
            return Application.persistentDataPath;
        }

        public static string GetWWWStreamPath()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
            {
                var tmpPath = "";
                tmpPath = GetWWWFileHead() + Application.streamingAssetsPath + "/";

                return tmpPath;
            }
            var tmpStr = GetWWWFileHead();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                {
                    tmpStr = tmpStr + Application.dataPath + "!/assets/";
                }
                    break;
                case RuntimePlatform.IPhonePlayer:
                {
                    tmpStr = tmpStr + Application.dataPath + "/Raw/";
                }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                {
                    tmpStr = tmpStr + Application.streamingAssetsPath + "/";
                }
                    break;
                default:
                {
                    tmpStr = tmpStr + Application.dataPath + GetRuntimeFolderAsestTail();
                }
                    break;
            }
            return tmpStr;
        }

        public static string GetRuntimeFolderAsestTail()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                {
                    return "/StreamingAssets/";
                }

                case RuntimePlatform.Android:
                {
                    return "!/assets/";
                }
                default:
                    return "/Raw/";
            }
        }

        /// <summary>
        ///     获取文件路径头
        /// </summary>
        /// <returns></returns>
        public static string GetWWWFileHead()
        {
            var tmpStr = "";
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                {
                    var tmpPath = "";
                    tmpPath = "file://";

                    return tmpPath;
                }
                case RuntimePlatform.Android:
                {
                    tmpStr = "jar:file://";
                }
                    break;
                    ;
                case RuntimePlatform.WindowsPlayer:
                {
                    tmpStr = "file:///";
                }
                    break;
                case RuntimePlatform.IPhonePlayer:
                {
                    tmpStr = "file://";
                }
                    break;
                default:
                {
                    tmpStr = GetRuntimeFolderWWWHead();
                }
                    break;
            }

            return tmpStr;
        }
    }
}