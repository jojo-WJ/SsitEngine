/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/12 10:24:03                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.IO;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class ChangeScriptTemplates : UnityEditor.AssetModificationProcessor
    {
        // 要添加的注释的内容  
        private static string annotationStr =
            @"/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：#UserName#                                              
*│　版    本：1.0.0                                                 
*│　创建时间：#CreateTime#                             
*└──────────────────────────────────────────────────────────────┘
*/
";

        private static string[] filterStrs = new string[]
        {
            "Scripts/Framework/ProtoMsg",
            "Scripts/Data",
        };

        public static void OnWillCreateAsset( string path )
        {
            // 排除“.meta”文件  
            path = path.Replace(".meta", "");

            // 路径过滤（主要对于工具生成的脚本进行有效过滤）
            for (int i = 0; i < filterStrs.Length; i++)
            {
                if (path.Contains(filterStrs[i]))
                {
                    Debug.Log($"filterStrs {path}");
                    return;
                }
            }


            // 如果是CS脚本，则进行添加注释处理  
            if (path.EndsWith(".cs"))
            {
                // 读取cs脚本的内容并添加到annotationStr 后面 
                string fileText = File.ReadAllText(path);

                if (fileText.Contains("作    者"))
                {
                    return;
                }

                annotationStr += fileText;

                // 把 #UserName# 替换成具体用户名称  
                annotationStr = annotationStr.Replace("#UserName#", GameReference.GetString(BDPreferences.UserName));

                // 把 #CreateTime# 替换成具体创建时间  
                annotationStr =
                    annotationStr.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                // 把内容重新写入脚本  
                File.WriteAllText(path, annotationStr);
            }
        }
    }
}