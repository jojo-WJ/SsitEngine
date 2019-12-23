/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/26 20:28:38                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class LogRedirect
    {
        private const string logTitle = "检测到错误:";

        private static readonly string[] ignores = new string[2]
        {
            "***********代码检测结果************",
            "*********************************"
        };

        private static object logListView;
        private static EditorWindow consoleWindow;
        private static FieldInfo logListViewCurrentRow;
        private static FieldInfo activeTextInfo;

        private static bool GetConsoleWindowListView()
        {
            if (logListView == null)
            {
                try
                {
                    var type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.ConsoleWindow");
                    consoleWindow =
                        type.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic)
                            .GetValue(null) as EditorWindow;
                    if (consoleWindow == null)
                    {
                        logListView = null;
                        Debug.Log("consoleWindow refelct is exception");
                        return false;
                    }
                    var field = type.GetField("m_ListView",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    logListView = field.GetValue(consoleWindow);
                    logListViewCurrentRow =
                        field.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
                    activeTextInfo =
                        type.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                catch (Exception ex)
                {
                    Debug.LogError("CodeCheck:" + ex);
                    return false;
                }
            }
            return true;
        }

        private static ErrorInfo GetErrorInfo()
        {
            var num = (int) logListViewCurrentRow.GetValue(logListView);
            var condition = activeTextInfo.GetValue(consoleWindow).ToString();
            condition = condition.Trim(' ', '\n', '\t');
            if (Array.FindIndex(ignores, n => condition.StartsWith(n)) >= 0)
            {
                return new ErrorInfo
                {
                    Location = string.Empty
                };
            }
            if (!condition.StartsWith("检测到错误:"))
            {
                return null;
            }
            var strArray = condition.Split('\n');
            if (strArray.Length < 7)
            {
                return null;
            }
            try
            {
                var errorInfo = new ErrorInfo
                {
                    Message = strArray[1].Trim(' ', '\n'),
                    Location = strArray[2].Trim(' ', '\n').Replace("位于", "")
                };
                errorInfo.Location = errorInfo.Location.Substring(errorInfo.Location.IndexOf("Assets/"));
                errorInfo.Line = int.Parse(strArray[3].Trim(' ', '\n').Replace("第", "").Replace("行", ""));
                errorInfo.RuleSpace = strArray[4].Trim(' ', '\n').Replace("规则空间:", "");
                errorInfo.RuleName = strArray[5].Trim(' ', '\n').Replace("规则:", "");
                errorInfo.RuleId = strArray[6].Trim(' ', '\n').Replace("规则ID:", "");
                return errorInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError("CodeCheck:" + ex);
                return null;
            }
        }

        [OnOpenAsset(0)]
        public static bool OnOpenAsset( int instanceID, int line )
        {
            if (!EditorWindow.focusedWindow.titleContent.text.Equals("Console"))
            {
                Debug.LogWarningFormat("CodeCheck:{0}", (object) "Focused window is not Console!");
                return false;
            }
            if (!GetConsoleWindowListView())
            {
                Debug.LogWarningFormat("CodeCheck:{0}", (object) "Get console window list view failed!");
                return false;
            }
            var errorInfo = GetErrorInfo();
            if (errorInfo == null)
            {
                return false;
            }
            var location = errorInfo.Location;
            if (location == string.Empty)
            {
                return true;
            }
            InternalEditorUtility.OpenFileAtLineExternal(
                Path.Combine(Path.GetDirectoryName(Application.dataPath), location.Trim()), errorInfo.Line);
            return true;
        }

        public sealed class ErrorInfo
        {
            public int Line;
            public string Location;
            public string Message;
            public string RuleId;
            public string RuleName;
            public string RuleSpace;
        }
    }
}