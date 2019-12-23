using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorFileUtility
{
    #region Members

    public static string DataPathInEditor => Application.dataPath;

    public static string DataPathInPlat => Application.persistentDataPath;

    public static string DataPathInStreaming => Application.streamingAssetsPath;

    #endregion

    #region Search

    /// <summary>
    /// 检测文件存在
    /// </summary>
    /// <param name="fileName">相对于Assets文件下的目录</param>
    /// <returns></returns>
    public static bool IsFileExists( string fileName )
    {
        if (fileName.Equals(string.Empty))
        {
            return false;
        }
        return File.Exists(GetFullPath(fileName));
    }

    /// <summary>
    /// 检测是否存在文件夹
    /// </summary>
    /// <param name="folderPath">相对Assets下的文件夹</param>
    /// <returns></returns>
    public static bool IsFolderExists( string folderPath )
    {
        if (folderPath.Equals(string.Empty))
        {
            return false;
        }
        return Directory.Exists(GetFullPath(folderPath));
    }

    #endregion

    #region Add

    /// 在Application.dataPath目录下创建文件
    public static void CreateFile( string fileName )
    {
        if (!IsFileExists(fileName))
        {
            CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

#if UNITY_4 || UNITY_5
            FileStream stream = File.Create(GetFullPath(fileName));
            stream.Close();
#else
            File.Create(GetFullPath(fileName));
#endif
        }
    }

    /// 创建文件夹
    public static void CreateFolder( string folderPath )
    {
        if (!IsFolderExists(folderPath))
        {
            Directory.CreateDirectory(GetFullPath(folderPath));

            AssetDatabase.Refresh();
        }
    }

    /// 在Assets下创建目录
    public static void CreateAssetFolder( string assetFolderPath )
    {
        if (!IsFolderExists(assetFolderPath))
        {
            var index = assetFolderPath.IndexOf("/");
            var offset = 0;
            var parentFolder = "Assets";
            while (index != -1)
            {
                if (!Directory.Exists(GetFullPath(assetFolderPath.Substring(0, index))))
                {
                    var guid = AssetDatabase.CreateFolder(parentFolder,
                        assetFolderPath.Substring(offset, index - offset));
                    // 将GUID(全局唯一标识符)转换为对应的资源路径。
                    AssetDatabase.GUIDToAssetPath(guid);
                }
                offset = index + 1;
                parentFolder = "Assets/" + assetFolderPath.Substring(0, offset - 1);
                index = assetFolderPath.IndexOf("/", index + 1);
            }

            AssetDatabase.Refresh();
        }
    }

    #endregion

    #region Copy

    /// 复制文件
    public static void CopyFile( string srcFileName, string destFileName )
    {
        if (IsFileExists(srcFileName) && !srcFileName.Equals(destFileName))
        {
            var index = destFileName.LastIndexOf("/");
            var filePath = string.Empty;

            if (index != -1)
            {
                filePath = destFileName.Substring(0, index);
            }

            if (!Directory.Exists(GetFullPath(filePath)))
            {
                Directory.CreateDirectory(GetFullPath(filePath));
            }

            File.Copy(GetFullPath(srcFileName), GetFullPath(destFileName), true);

            AssetDatabase.Refresh();
        }
    }

    /// 复制文件夹
    public static void CopyFolder( string srcFolderPath, string destFolderPath )
    {
#if !UNITY_WEBPLAYER
        if (!IsFolderExists(srcFolderPath))
        {
            return;
        }

        CreateFolder(destFolderPath);


        srcFolderPath = GetFullPath(srcFolderPath);
        destFolderPath = GetFullPath(destFolderPath);

        // 创建所有的对应目录
        foreach (var dirPath in Directory.GetDirectories(srcFolderPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(srcFolderPath, destFolderPath));
        }

        // 复制原文件夹下所有内容到目标文件夹，直接覆盖
        foreach (var newPath in Directory.GetFiles(srcFolderPath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(srcFolderPath, destFolderPath), true);
        }

        AssetDatabase.Refresh();
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }

    /// 复制Assets下内容
    public static void CopyAsset( string srcAssetName, string destAssetName )
    {
        if (IsFileExists(srcAssetName) && !srcAssetName.Equals(destAssetName))
        {
            var index = destAssetName.LastIndexOf("/");
            var filePath = string.Empty;

            if (index != -1)
            {
                filePath = destAssetName.Substring(0, index + 1);
                //Create asset folder if needed
                CreateAssetFolder(filePath);
            }


            AssetDatabase.CopyAsset(GetFullAssetPath(srcAssetName), GetFullAssetPath(destAssetName));
            AssetDatabase.Refresh();
        }
    }

    #endregion

    #region Delete

    /// 删除文件
    public static void DeleteFile( string fileName )
    {
        if (IsFileExists(fileName))
        {
            File.Delete(GetFullPath(fileName));

            AssetDatabase.Refresh();
        }
    }

    /// 删除文件夹
    public static void DeleteFolder( string folderPath )
    {
#if !UNITY_WEBPLAYER
        if (IsFolderExists(folderPath))
        {
            Directory.Delete(GetFullPath(folderPath), true);

            AssetDatabase.Refresh();
        }
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::DeleteFolder is innored under wep player platfrom");
#endif
    }

    /// 删除Assets下内容
    public static void DeleteAsset( string assetName )
    {
        if (IsFileExists(assetName))
        {
            AssetDatabase.DeleteAsset(GetFullAssetPath(assetName));
            AssetDatabase.Refresh();
        }
    }

    #endregion

    #region Read And Write

    /// 写入数据到对应文件
    public static void Write( string fileName, string contents )
    {
        CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

        TextWriter tw = new StreamWriter(GetFullPath(fileName), false);
        tw.Write(contents);
        tw.Close();
        AssetDatabase.Refresh();
    }

    /// 从对应文件读取数据
    public static string Read( string fileName )
    {
#if !UNITY_WEBPLAYER
        if (IsFileExists(fileName))
        {
            return File.ReadAllText(GetFullPath(fileName));
        }
        return "";
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }

    #endregion

    #region interval helper

    /// <summary>
    /// 返回Application.dataPath下完整目录
    /// </summary>
    /// <param name="srcName"></param>
    /// <returns></returns>
    public static string GetFullPath( string srcName )
    {
        if (srcName.Equals(string.Empty))
        {
            return Application.dataPath;
        }

        if (srcName[0].Equals('/'))
        {
            srcName.Remove(0, 1);
        }

        return Application.dataPath + "/" + srcName;
    }

    /// <summary>
    /// 获取Assets下完整路径
    /// </summary>
    /// <param name="assetName">相对于Assets文件的目录</param>
    /// <returns></returns>
    public static string GetFullAssetPath( string assetName )
    {
        if (assetName.Equals(string.Empty))
        {
            return "Assets/";
        }

        if (assetName[0].Equals('/'))
        {
            assetName.Remove(0, 1);
        }

        return "Assets/" + assetName;
    }

    #endregion
}