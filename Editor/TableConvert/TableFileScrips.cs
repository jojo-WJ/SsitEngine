using UnityEngine;

namespace SsitEngine.Editor
{
    public class TableFileScrips : ScriptableObject
    {
        //批处理文件路径
        public string WorkSpacePath = "";
        //批处理文件相对路径
        public string WorkSpaceRelationPath = "Table";
        //脚本文件导出路径
        public string ScriptExportPath = "";
        //Josn文件导出路径
        public string JsonExportPath = "";
        //二进制文件导出路径
        public string BinaryExportPath = "";
        //Lua文件导出路径
        public string LuaExportPath = "";
        //是否导出Lua
        public bool mIsExportLua = false;
        //是否导出LuaForBit
        public bool mIsExportLuaForBit = false;
        //是否导出Json
        public bool mIsExportJson = true;
        //是否导出Json
        public bool mIsExportResources = false;
        //批处理文件名称
        public string[] FileNames;

    }
}