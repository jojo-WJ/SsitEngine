using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SsitEngine.Editor
{
    public sealed class TableTool
    {
        //命令行所在目录
        private static string CommandPathFolder = "";

        //二进制文件所在目录
        public static readonly string BinaryFloderPath = "/Packages/Table";

        //数据模型脚本所在目录
        public static readonly string DataScriptsFolderPath = "/Scripts/Data/Model";

        //Lua文件所在目录
        public static readonly string LuasFolderPath = "/uLua/Lua/Table";

        //Json文件所在目录
        public static readonly string JsonFolderResPath = "/Resources/JsonData";

        public static readonly string JsonFolderDefaultPath = "/Art/JsonData";

        public static void TableConvert( string path )
        {
            IntiFilePath();
            CheckFolder();
            Process proc = null;
            if (!File.Exists(CommandPathFolder + path))
            {
                Debug.LogError("批处理命令不存在::" + CommandPathFolder + "Path::" + path);
                return;
            }

            try
            {
                proc = new Process();
                proc.StartInfo.WorkingDirectory = CommandPathFolder;
                proc.StartInfo.FileName = path;

                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                proc.Start();

                proc.WaitForExit();
                //EditorUtility.DisplayDialog("导出结果", "导出成功!", "确定");
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace);
            }

            var dir = new DirectoryInfo(Application.dataPath + BinaryFloderPath); //目录
            if (dir.Exists) //目录存在
            {
                foreach (var file in dir.GetFiles("*.bin")) //遍历后缀名为bin的文件
                {
                    //计算相应的以bytes为后缀的同名文件
                    var byteFile = file.FullName.Substring(0, file.FullName.LastIndexOf(".")) + ".bytes";
                    if (File.Exists(file.FullName))
                    {
                        File.Delete(byteFile); //删除旧bytes文件
                    }

                    file.MoveTo(byteFile); //bin文件重命名为bytes文件
                }
            }

            Fresh();
        }

        public static void TableConvert( FolderTree.Data data, string commandpath, TableFileScrips mConfigInfo )
        {
            Process proc = null;
            if (!Directory.Exists(mConfigInfo.WorkSpacePath))
            {
                Debug.LogError("批处理工作路径不存在::" + "Path::" + mConfigInfo.WorkSpacePath);
                return;
            }

            var batPath = commandpath + ".bat";

            if (!File.Exists(batPath))
            {
                Debug.LogError("批处理命令不存在::" + batPath);
                return;
            }

            try
            {
                proc = new Process();
                proc.StartInfo.WorkingDirectory = mConfigInfo.WorkSpacePath;

                proc.StartInfo.FileName = batPath;

                //proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                proc.Start();

                proc.WaitForExit();
                //EditorUtility.DisplayDialog( "导出结果", "导出成功!", "确定" );
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace);
            }

            //修正c#
            var scripPath = mConfigInfo.ScriptExportPath + "/" + data.name + ".cs";
            if (File.Exists(scripPath))
            {
                ReToC(scripPath, mConfigInfo.mIsExportJson);
            }

            //修正Lua
            if (!mConfigInfo.mIsExportLua)
            {
                if (Directory.Exists(mConfigInfo.LuaExportPath))
                {
                    Directory.Delete(mConfigInfo.LuaExportPath, true);
                }
            }
            else
            {
                //判断当前的data是否是文件夹
                if (data.isFolder)
                {
                    var tempPaths = EditorFileUtility.GetFilesByPattern(data.relationPath, ".xlsx");
                    foreach (var item in tempPaths)
                    {
                        Debug.Log(item);
                    }
                }
                else
                {
                    var luaPath = mConfigInfo.LuaExportPath + "/" + data.name + ".lua";

                    if (File.Exists(luaPath))
                    {
                        var refName = data.name + "Table";
                        ToLua(luaPath, data.name, ref refName);
                    }
                }
            }

            //DirectoryInfo dir = new DirectoryInfo(mConfigInfo.BinaryExportPath);//目录
            //if (dir.Exists)//目录存在
            //{
            //    foreach (var file in dir.GetFiles("*.bin"))//遍历后缀名为bin的文件
            //    {
            //        //计算相应的以bytes为后缀的同名文件
            //        string byteFile = file.FullName.Substring(0, file.FullName.LastIndexOf(".")) + ".bytes";
            //        if (File.Exists(file.FullName))
            //        {
            //            File.Delete(byteFile);//删除旧bytes文件
            //        }
            //        file.MoveTo(byteFile);//bin文件重命名为bytes文件
            //    }
            //}
            Fresh();
        }


        private static void Fresh()
        {
            //UnityEditor.EditorApplication.ExecuteMenuItem("Assets/Refresh");
            AssetDatabase.Refresh();
        }

        private static void IntiFilePath()
        {
            //DirectoryInfo d1 = new DirectoryInfo(Application.dataPath + BinaryFloderPath);
            var path = Application.dataPath;

            path = path.Replace("Assets", "Table");
            path = Path.GetFullPath(path) + "\\";
            CommandPathFolder = path;
        }

        public static void CheckFolder()
        {
            //DirectoryInfo d1 = new DirectoryInfo(Application.dataPath + BinaryFloderPath);
            //if (!d1.Exists) { d1.Create(); }
            var d1 = new DirectoryInfo(Application.dataPath + DataScriptsFolderPath);
            if (!d1.Exists)
            {
                d1.Create();
            }

            //d1 = new DirectoryInfo(Application.dataPath + LuasFolderPath);
            //if (!d1.Exists) { d1.Create(); }
            //d1 = new DirectoryInfo(Application.dataPath + JsonFolderResPath);
            //if (!d1.Exists)
            //{
                d1.Create();
            //}
        }

        #region C#代码修正

        private static void ReToC( string path, bool isJson )
        {
            var cr = new CodeReGeneration(path);
            if (isJson)
            {
                //cr.Replace("public tabtoy.Logger TableLogger = new tabtoy.Logger();", "");
                ToJsonC(path);
            }
            else
            {
                cr.Replace("public tabtoy.Logger TableLogger = new tabtoy.Logger();",
                    "public Tabtoy.Logger TableLogger = Tabtoy.Logger.Instance();");
            }
        }

        #endregion

        #region ExportLuaTools

        public static string ToLua( string path, string name, ref string luaTableName )
        {
            var cContent = EditorFileUtility.ReadFile(path);

            if (!string.IsNullOrEmpty(cContent))
            {
                var cc = SpitContentC(ref cContent, StringSplitOptions.None);

                var preTableName = "";

                var iEnumerator = cc.GetEnumerator();
                var index = -1;
                while (iEnumerator.MoveNext())
                {
                    index++;
                    var cur = (string) iEnumerator.Current;
                    if (cur.StartsWith("--"))
                    {
                        continue;
                    }
                    if (cur.StartsWith("local"))
                    {
                        cur = cur.Remove(0, 5);
                        var vv = cur.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                        preTableName = vv[0];
                        cc[index] = luaTableName + " = " + luaTableName + " or {}";
                        iEnumerator.MoveNext();
                        index++;
                        cur = (string) iEnumerator.Current;
                        cur = cur.Replace("\t", "");
                        cur = cur.Insert(0, "local ");
                        cc[index] = cur;
                        while (iEnumerator.MoveNext())
                        {
                            index++;
                            cur = (string) iEnumerator.Current;

                            cur = cur.Replace("\t\t", "\t");
                            cc[index] = cur;

                            if (cur.Equals("}"))
                            {
                                cc[index] = "";

                                cc[index - 2] = "}";
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(preTableName))
                    {
                        cur = (string) iEnumerator.Current;

                        var partical = preTableName + "." + name;
                        if (cur.Contains("(") && cur.Contains(partical))
                        {
                            cur = cur.Replace(partical, name);
                        }
                        else
                        {
                            cur = cur.Replace(preTableName, luaTableName);
                        }

                        cc[index] = cur;
                    }
                }

                var ret = string.Join("\n", cc);

                //写入原路径
                EditorFileUtility.WriteFile(path, ret);
            }


            return cContent;
        }

        /// <summary>
        /// 按行拆分
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string[] SpitContentC( ref string target, StringSplitOptions options )
        {
            return target.Split(new[] {"\n"}, options);
        }


        #region Fitter

        public static void filterKeyWord( ref string[] targetArray )
        {
            for (var i = 0; i < targetArray.Length; i++)
            {
            }
        }


        public static string[] filterWord =
        {
            //Define
            "#region",
            "#endregion",
            "#if",
            "#else",
            "#endif",


            //class Info
            "using",
            "namespace"
        };

        public static string[] NotesWord =
        {
            //注释
            "//",
            "/*",
            "*/"
        };

        public static string[] MethodWord =
        {
            //方法
            "private",
            "public",
            "static",
            "void"
        };

        public static string[] ScriptBlockWord =
        {
            //方法
            "if",
            "else if",
            "else",
            "while",
            "do",
            "for",
            "continue",
            "foreach"
        };

        #endregion

        #endregion


        #region ExportJsonTools

        public static string ToJsonC( string path )
        {
            var cContent = EditorFileUtility.ReadFile(path);
            var temp = new List<string>();
            if (!string.IsNullOrEmpty(cContent))
            {
                var cc = SpitContentC(ref cContent, StringSplitOptions.None);

                var preTableName = "";

                var iEnumerator = cc.GetEnumerator();

                while (iEnumerator.MoveNext())
                {
                    var cur = (string) iEnumerator.Current;
                    if (cur.Contains("public tabtoy.Logger TableLogger "))
                    {
                        cur = cur.Replace("public tabtoy.Logger TableLogger = new tabtoy.Logger();",
                            "public Tabtoy.Logger TableLogger = Tabtoy.Logger.Instance();");
                        temp.Add(cur);
                        continue;
                    }

                    if (cur.Contains("public partial class"))
                    {
                        temp.Add("  [System.Serializable]");
                        temp.Add(cur);
                    }
                    else if (cur.Contains("#region Deserialize code"))
                    {
                        //获取类名
                        var TypeName = string.Empty;

                        while (iEnumerator.MoveNext())
                        {
                            cur = (string) iEnumerator.Current;

                            if (cur.Contains("public static void Deserialize"))
                            {
                                var tempHeader = cur.Split(new[] {" ", "(", ","},
                                    StringSplitOptions.RemoveEmptyEntries);
                                TypeName = tempHeader[4];
                            }

                            //获取字典块
                            if (cur.Contains("// Build"))
                            {
                                var sb1 = new StringBuilder();
                                var sb2 = new StringBuilder();
                                var additiveContext = string.Empty;
                                sb1.AppendLine(cur);
                                sb2.AppendLine(cur);
                                while (iEnumerator.MoveNext())
                                {
                                    cur = (string) iEnumerator.Current;
                                    sb1.AppendLine(cur);
                                    if (!string.IsNullOrEmpty(cur) && cur.Contains("var"))
                                    {
                                        sb2.AppendLine(cur);
                                        var listParams = cur.Split(new[] {".", "["},
                                            StringSplitOptions.RemoveEmptyEntries);
                                        var param = listParams[1];
                                        additiveContext = string.Format("\t\t\t\t\tthis.{0}.Add(ins.{1}[i]);", param,
                                            param);
                                        //sb2.AppendLine( tempt );
                                    }
                                    else if (!string.IsNullOrEmpty(cur) && cur.Contains("Add"))
                                    {
                                        //string tempt = cur.Replace( "ins", "this" );
                                        var listParams1 = cur.Split(new[] {"(", ","},
                                            StringSplitOptions.RemoveEmptyEntries);
                                        var key = listParams1[1];
                                        var listParams2 = cur.Split(new[] {"."},
                                            StringSplitOptions.RemoveEmptyEntries);
                                        var dic = listParams2[1];
                                        var llr1 = string.Format("\t\t\t\tif(!{0}.ContainsKey({1}))", dic, key);
                                        sb2.AppendLine(llr1);

                                        sb2.AppendLine("\t\t\t\t{");
                                        if (!string.IsNullOrEmpty(additiveContext))
                                        {
                                            sb2.AppendLine(additiveContext);
                                        }

                                        var llr2 = string.Format("\t\t\t\t\t{0}.Add({1}, element);", dic, key);
                                        sb2.AppendLine(llr2);


                                        sb2.AppendLine("\t\t\t\t}");
                                    }
                                    else
                                    {
                                        sb2.AppendLine(cur);
                                    }

                                    if (cur.Contains("}"))
                                    {
                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(TypeName))
                                {
                                    var temp1 = BuildDeserializeCode;
                                    temp1 = temp1.Replace("arg0", TypeName);
                                    temp1 = temp1.Replace("arg1", sb1.ToString());
                                    temp.Add(temp1);

                                    var temp2 = BuildDeserializeReCode;
                                    temp2 = temp2.Replace("arg0", TypeName);
                                    temp2 = temp2.Replace("arg2", sb2.ToString());
                                    temp.Add(temp2);
                                }
                            }

                            if (cur.Contains("#endregion"))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        temp.Add(cur);
                    }
                }

                var ret = string.Join("\n", temp.ToArray());

                //写入原路径
                EditorFileUtility.WriteFile(path, ret);
            }


            return cContent;
        }

        private static readonly string BuildDeserializeCode =
            @"      public static void Deserialize( arg0 ins)
		{
            arg1
        }
        ";

        private static readonly string BuildDeserializeReCode =
            @"      public void DeserializeRe( arg0 ins)
		{
            arg2
        }
        ";

        #endregion

        //public static void TableConvert(string workSpace, string command)
        //{
        //    Process proc = null;
        //    if (string.IsNullOrEmpty(command))
        //    {
        //        UnityEngine.Debug.LogError("批处理命令不存在::" + command);
        //        return;
        //    }
        //    UnityEngine.Debug.Log(command);

        //    try
        //    {
        //        proc = new Process();
        //        proc.StartInfo.WorkingDirectory = workSpace;

        //        proc.StartInfo.FileName = "cmd.exe";

        //        proc.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
        //        proc.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
        //        proc.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
        //        proc.StartInfo.RedirectStandardError = true;//重定向标准错误输出

        //        //proc.StartInfo.CreateNoWindow = true;
        //        proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //        proc.Start();

        //        //向cmd窗口发送输入信息
        //        proc.StandardInput.WriteLine(command/* + "&exit"*/);

        //        proc.StandardInput.AutoFlush = true;
        //        //p.StandardInput.WriteLine("exit");
        //        //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
        //        //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

        //        string output = proc.StandardOutput.ReadToEnd();

        //        proc.WaitForExit();
        //        proc.Close();
        //        EditorUtility.DisplayDialog("导出结果", "导出成功!", "确定");

        //        UnityEngine.Debug.Log(output);
        //    }
        //    catch (Exception ex)
        //    {
        //        UnityEngine.Debug.LogErrorFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
        //    }

        //    DirectoryInfo dir = new DirectoryInfo(Application.dataPath + BinaryFloderPath);//目录
        //    if (dir.Exists)//目录存在
        //    {
        //        foreach (var file in dir.GetFiles("*.bin"))//遍历后缀名为bin的文件
        //        {
        //            //计算相应的以bytes为后缀的同名文件
        //            string byteFile = file.FullName.Substring(0, file.FullName.LastIndexOf(".")) + ".bytes";
        //            if (File.Exists(file.FullName))
        //            {
        //                File.Delete(byteFile);//删除旧bytes文件
        //            }
        //            file.MoveTo(byteFile);//bin文件重命名为bytes文件
        //        }
        //    }
        //    Fresh();
        //}
    }
}