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
        private static string[] ignores = new string[2]
        {
            "***********代码检测结果************",
            "*********************************"
        };

        private static object logListView = null;
        private const string logTitle = "检测到错误:";
        private static EditorWindow consoleWindow;
        private static System.Reflection.FieldInfo logListViewCurrentRow;
        private static System.Reflection.FieldInfo activeTextInfo;

        private static bool GetConsoleWindowListView()
        {
            if (LogRedirect.logListView == null)
            {
                try
                {
                    System.Type type = Assembly.GetAssembly(typeof(EditorWindow)).GetType("UnityEditor.ConsoleWindow");
                    LogRedirect.consoleWindow =
                        type.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic)
                            .GetValue(null) as EditorWindow;
                    if (consoleWindow == null)
                    {
                        LogRedirect.logListView = null;
                        Debug.Log("consoleWindow refelct is exception");
                        return false;
                    }
                    System.Reflection.FieldInfo field = type.GetField("m_ListView",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    LogRedirect.logListView = field.GetValue(consoleWindow);
                    LogRedirect.logListViewCurrentRow =
                        field.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
                    LogRedirect.activeTextInfo =
                        type.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                catch (Exception ex)
                {
                    Debug.LogError("CodeCheck:" + ex.ToString());
                    return false;
                }
            }
            return true;
        }

        private static LogRedirect.ErrorInfo GetErrorInfo()
        {
            int num = (int) LogRedirect.logListViewCurrentRow.GetValue(LogRedirect.logListView);
            string condition = LogRedirect.activeTextInfo.GetValue(consoleWindow).ToString();
            condition = condition.Trim(' ', '\n', '\t');
            if (Array.FindIndex<string>(LogRedirect.ignores, n => condition.StartsWith(n)) >= 0)
                return new LogRedirect.ErrorInfo()
                {
                    Location = string.Empty
                };
            if (!condition.StartsWith("检测到错误:"))
                return null;
            string[] strArray = condition.Split('\n');
            if (strArray.Length < 7)
                return null;
            try
            {
                LogRedirect.ErrorInfo errorInfo = new LogRedirect.ErrorInfo()
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
                Debug.LogError("CodeCheck:" + ex.ToString());
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
            if (!LogRedirect.GetConsoleWindowListView())
            {
                Debug.LogWarningFormat("CodeCheck:{0}", (object) "Get console window list view failed!");
                return false;
            }
            LogRedirect.ErrorInfo errorInfo = LogRedirect.GetErrorInfo();
            if (errorInfo == null)
                return false;
            string location = errorInfo.Location;
            if (location == string.Empty)
                return true;
            InternalEditorUtility.OpenFileAtLineExternal(
                Path.Combine(Path.GetDirectoryName(Application.dataPath), location.Trim()), errorInfo.Line);
            return true;
        }

        public sealed class ErrorInfo
        {
            public string Message;
            public string Location;
            public int Line;
            public string RuleSpace;
            public string RuleName;
            public string RuleId;
        }
    }
}