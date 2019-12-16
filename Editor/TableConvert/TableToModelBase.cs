using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class TableToModelBase
    {

        List<string> mSubClassList;
        private readonly string ExportPath = "/Scripts/Data/Common";
        private readonly string ScriptsName = "ModelBase.cs";

        public TableToModelBase()
        {
            mSubClassList = new List<string>();

            InitMaps();
        }

        void InitMaps()
        {
            if (Directory.Exists(Application.dataPath + TableTool.DataScriptsFolderPath))
            {
                DirectoryInfo folder = new DirectoryInfo(Application.dataPath + TableTool.DataScriptsFolderPath);

                foreach (var ff in folder.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    string fName = Path.GetFileNameWithoutExtension(ff.Name);
                    mSubClassList.Add(fName + "Model");
                }
            }

        }


        public void ExportModelBase()
        {
            string content = CombineModel();

            if (!string.IsNullOrEmpty(content))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + ExportPath);
                EditorFileUtility.WriteFile(Application.dataPath + ExportPath + "/" + ScriptsName, content);
            }

            UnityEditor.AssetDatabase.Refresh();
        }


        public string CombineModel()
        {

            StringBuilder sb = new StringBuilder();

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




        void WriteBaseCalss( StringBuilder sb )
        {
            sb.AppendLine("  public abstract class ModelBase");
            WriteClassBegin(sb);
            sb.AppendLine("     public virtual void Deserialized( ModelBase ins ) { }");
            sb.AppendLine("    public virtual void DeserializedRe( ModelBase ins ) { }");


            WriteClassEnd(sb);
        }

        void WriteSubClass( StringBuilder sb )
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
                sb.AppendFormat("       {0},", item.Replace("Model",""));
                sb.AppendLine();
            }
            WriteClassEnd(sb);

        }

        void WriteSpaceBegin( StringBuilder sb )
        {
            sb.AppendLine("{");
        }

        void WriteSpaceEnd( StringBuilder sb )
        {
            sb.AppendLine("}");
        }

        void WriteClassBegin( StringBuilder sb )
        {
            sb.AppendLine("   {");
        }

        void WriteClassEnd( StringBuilder sb )
        {
            sb.AppendLine("   }");
        }


        void WriteMethodBegin( StringBuilder sb )
        {
            sb.AppendLine("     {");
        }

        void WriteMethodEnd( StringBuilder sb )
        {
            sb.AppendLine("     }");
        }
    }
}