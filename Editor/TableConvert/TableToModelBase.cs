using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class TableToModelBase
    {
        private readonly string ExportPath = "/Scripts/Data/Common";
        private readonly string ScriptsName = "ModelBase.cs";

        private readonly List<string> mSubClassList;

        public TableToModelBase()
        {
            mSubClassList = new List<string>();

            InitMaps();
        }

        private void InitMaps()
        {
            var tempPath = Application.dataPath + TableTool.DataScriptsFolderPath;
            if (Directory.Exists(tempPath))
            {
                var folder = new DirectoryInfo(tempPath);

                foreach (var ff in folder.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    var fName = Path.GetFileNameWithoutExtension(ff.Name);
                    mSubClassList.Add(fName + "Model");
                }
            }
        }


        public void ExportModelBase()
        {
            var content = CombineModel();

            if (!string.IsNullOrEmpty(content))
            {
                Directory.CreateDirectory(Application.dataPath + ExportPath);
                EditorFileUtility.WriteFile(Application.dataPath + ExportPath + "/" + ScriptsName, content);
            }

            AssetDatabase.Refresh();
        }


        public string CombineModel()
        {
            var sb = new StringBuilder();

            //写头部引用
            sb.AppendLine("using Tabtoy;");
            sb.AppendLine("using SsitEngine.Data;");
            sb.AppendLine();

            //写命名空间
            sb.AppendLine("namespace Table");

            WriteSpaceBegin(sb);
            //写基类
            //WriteBaseCalss(sb);

            //写子类
            WriteSubClass(sb);

            //写枚举
            WriteTableEnum(sb);

            WriteSpaceEnd(sb);

            return sb.ToString();
        }


        private void WriteBaseCalss( StringBuilder sb )
        {
            sb.AppendLine("  public abstract class ModelBase");
            WriteClassBegin(sb);
            sb.AppendLine("     public virtual void Deserialized( ModelBase ins ) { }");
            sb.AppendLine("    public virtual void DeserializedRe( ModelBase ins ) { }");


            WriteClassEnd(sb);
        }

        private void WriteSubClass( StringBuilder sb )
        {
            foreach (var item in mSubClassList)
            {
                sb.AppendFormat("   public partial class {0} : ModelBase", item);
                sb.AppendLine();

                WriteClassBegin(sb);
                {
                    sb.AppendLine("     public override void Deserialized(ModelBase ins)");
                    {
                        WriteMethodBegin(sb);
                        sb.AppendFormat("       Deserialize(ins as {0});", item);
                        sb.AppendLine();
                        WriteMethodEnd(sb);
                    }

                    sb.AppendLine("     public override void DeserializedRe(ModelBase ins)");
                    {
                        WriteMethodBegin(sb);
                        sb.AppendFormat("       this.DeserializeRe(ins as {0});", item);
                        sb.AppendLine();
                        WriteMethodEnd(sb);
                    }
                }


                WriteClassEnd(sb);
            }
        }

        private void WriteTableEnum( StringBuilder sb )
        {
            sb.AppendLine("   public enum EnTableType");
            WriteClassBegin(sb);

            foreach (var item in mSubClassList)
            {
                sb.AppendFormat("       {0},", item.Replace("Model", ""));
                sb.AppendLine();
            }
            WriteClassEnd(sb);
        }

        private void WriteSpaceBegin( StringBuilder sb )
        {
            sb.AppendLine("{");
        }

        private void WriteSpaceEnd( StringBuilder sb )
        {
            sb.AppendLine("}");
        }

        private void WriteClassBegin( StringBuilder sb )
        {
            sb.AppendLine("   {");
        }

        private void WriteClassEnd( StringBuilder sb )
        {
            sb.AppendLine("   }");
        }


        private void WriteMethodBegin( StringBuilder sb )
        {
            sb.AppendLine("     {");
        }

        private void WriteMethodEnd( StringBuilder sb )
        {
            sb.AppendLine("     }");
        }
    }
}