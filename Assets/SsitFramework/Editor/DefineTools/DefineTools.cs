/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 17:35:12                     
*└──────────────────────────────────────────────────────────────┘
*/
using System.Collections.Generic;
using UnityEditor;

namespace SsitEngine.Editor
{
    public class DefineTools
    {
        /// <summary>
        /// 设置编译条件
        /// </summary>
        /// <param name="defineName"></param>
        /// <param name="enable"></param>
        public static void SetEnabled( string defineName, bool enable )
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = GetDefinesList(buildTargetGroup);
            if (enable)
            {
                if (defines.Contains(defineName))
                {
                    return;
                }
                defines.Add(defineName);
            }
            else
            {
                if (!defines.Contains(defineName))
                {
                    return;
                }
                while (defines.Contains(defineName))
                {
                    defines.Remove(defineName);
                }
            }
            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesString);
        }

        /// <summary>
        /// 获取编译条件是否存在
        /// </summary>
        /// <param name="defineName"></param>
        /// <returns></returns>
        public static bool GetEnabled( string defineName )
        {
            return GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(defineName);
        }

        /// <summary>
        /// 获取编译组的编译条件
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static List<string> GetDefinesList( BuildTargetGroup group )
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }
    }
}
