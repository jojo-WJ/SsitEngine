using UnityEngine;

namespace SsitEngine.Editor
{
    public class TableFileScrips : ScriptableObject
    {
        //二进制文件导出路径
        public string BinaryExportPath = "";

        //批处理文件名称
        public string[] FileNames;

        //Josn文件导出路径
        public string JsonExportPath = "";

        //Lua文件导出路径
        public string LuaExportPath = "";

        //是否导出Json
        public bool mIsExportJson = true;

        //是否导出Lua
        public bool mIsExportLua;

        //是否导出LuaForBit
        public bool mIsExportLuaForBit;

        //是否导出Json
        public bool mIsExportResources;

        //脚本文件导出路径
        public string ScriptExportPath = "";

        //批处理文件路径
        public string WorkSpacePath = "";

        //批处理文件相对路径
        public string WorkSpaceRelationPath = "Table";
    }
}