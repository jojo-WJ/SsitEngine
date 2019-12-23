//using System.Text.RegularExpressions;
//using SsitEngine.Editor;
//using SsitEngine.Editor.Utility;
//using UnityEditor;
//using UnityEditor.Experimental;
///**
//*┌──────────────────────────────────────────────────────────────┐
//*│　描   述：                                                    
//*│　作   者：xuXin                                              
//*│　版   本：1.0.0                                                 
//*│　创建时间：2019/4/10 20:05:31                     
//*└──────────────────────────────────────────────────────────────┘
//*/

//using UnityEngine;

//public class AssetImportUtils : AssetPostprocessor
//{

//    static string basePath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

//    static void OnPostprocessAllAssets(           // 这个函数必须为静态的，其他可以不是！
//        string[] importedAssets,
//        string[] deletedAssets,
//        string[] movedAssets,
//        string[] movedFromAssetPaths )
//    {
//        foreach (var path in importedAssets)
//        {
//            // 判断文件是不是配置文件 .csv, json的.txt (个人角色json的配置文件就是以.json为后缀名是最为合理的！)
//            //if (path.EndsWith(".csv") || path.EndsWith(".txt") || path.EndsWith(".json"))
//            //{
//            //    string tempP = basePath + path;
//            //    System.Text.Encoding encode;
//            //    using (System.IO.FileStream fs = new System.IO.FileStream(tempP, System.IO.FileMode.Open, System.IO.FileAccess.Read))
//            //    {
//            //        encode = GetFileEncodeType(fs);
//            //    }

//            //    if (System.Text.Encoding.UTF8 != encode)
//            //    {
//            //        Debug.LogWarning("亲！配置文件" + tempP + "的编码格式不是UTF-8格式呦");
//            //        //// 转为 utf-8
//            //        //string str = File.ReadAllText(path, Encoding.Default);   // 转换没有问题, UTF8读就是乱码！！！
//            //        //File.WriteAllText(tempP, str, Encoding.UTF8);           
//            //    }
//            //}
//            if (path.EndsWith(".cs"))
//            {
//                //Object o = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                //Debug.Log("o type" + o.GetType());
//                string tempP = basePath + path;
//                using (System.IO.FileStream fs = new System.IO.FileStream(tempP, System.IO.FileMode.Open, System.IO.FileAccess.Read))
//                {
//                    System.IO.StreamReader br = new System.IO.StreamReader(fs);
//                    string context = br.ReadToEnd();
//                    //以下方法仅供编辑时使用（重新启动后失效，若想实现需要进一步去改写脚本Meta文件）
//                    if (Regex.IsMatch(context, ": DataBase"))
//                    {
//                        Object o = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                        GizmosIconUtils.SetIcon(o, GUIUtils.LoadEditorIcon("Data.png"));
//                    }
//                    else if (Regex.IsMatch(context, ": DataProxy"))
//                    {
//                        Object o = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                        GizmosIconUtils.SetIcon(o, GUIUtils.LoadEditorIcon("DataProxy.png"));
//                    }
//                }
//            }
//        }
//        //		for (var i=0;i<movedAssets.Length;i++)
//        //			Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
//    }

//    /// <summary>
//    /// 判断配置文件的编码格式是不是utf-8
//    /// </summary>
//    /// <returns>The file encode type.</returns>
//    /// <param name="filename">文件全路径.</param>
//    /// 代码中没判断内容是不是空
//    /// 检查时，csv文件不能用 office打开（因为独占）
//    static public System.Text.Encoding GetFileEncodeType( System.IO.FileStream fs )
//    {
//        System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
//        byte[] buffer = br.ReadBytes(2);

//        if (buffer[0] >= 0xEF)
//        {
//            if (buffer[0] == 0xEF && buffer[1] == 0xBB)
//            {
//                return System.Text.Encoding.UTF8;
//            }
//            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
//            {
//                return System.Text.Encoding.BigEndianUnicode;
//            }
//            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
//            {
//                return System.Text.Encoding.Unicode;
//            }
//            else
//            {
//                return System.Text.Encoding.Default;
//            }
//        }
//        else
//        {
//            return System.Text.Encoding.Default;
//        }
//        br.Close();
//        fs.Close();
//    }


//}

