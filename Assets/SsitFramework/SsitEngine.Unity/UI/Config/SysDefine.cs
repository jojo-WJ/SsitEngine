/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述： UI模块常量定义文件                                                   
*│　作   者： Jusam                                              
*│　版   本： 1.0.0                                                 
*│　创建时间： 2019/04/29                      
*└──────────────────────────────────────────────────────────────┘
*/


namespace SsitEngine.Unity.UI
{
    #region 系统枚举类型 

    /// <summary>
    ///     UI窗体位置类型
    /// </summary>
    public enum UIFormType
    {
        /// <summary>
        ///     普通窗体
        ///     窗体一般处于UI序列栈中的最底层的位置
        /// </summary>
        Normal,

        /// <summary>
        ///     固定功能窗体
        ///     窗体一般处于普通窗体之上且不会屏蔽普通窗体的事件响应
        ///     一般用于具有多个功能模块的窗体的同屏显示
        /// </summary>
        Fixed,

        /// <summary>
        ///     弹窗/警告/错误等提示窗体
        ///     窗体一般处于普通窗体和功能窗体之上且会屏蔽普事件响应
        /// </summary>
        PopUp,

        Custom1,
        Custom2,
        Custom3
    }

    /// <summary>
    ///     UI窗体显示类型
    /// </summary>
    public enum UIFormSHowMode
    {
        /// <summary>
        ///     普通，正常显示
        /// </summary>
        Normal,

        /// <summary>
        ///     反向切换
        ///     2个窗体之间的互斥显示
        /// </summary>
        ReverseChange,

        /// <summary>
        ///     隐藏其他
        /// </summary>
        HideOther,

        /// <summary>
        ///     同类型同时只能显示一个
        /// </summary>
        Single
    }

    /// <summary>
    ///     UI窗体透明类型
    /// </summary>
    public enum UIFormLucencyType
    {
        /// <summary>
        ///     完全透明不能穿透
        /// </summary>
        Lucency,

        /// <summary>
        ///     半透明不能穿透
        /// </summary>
        Translucence,

        /// <summary>
        ///     低透明度不能穿透
        /// </summary>
        Impenetrable,

        /// <summary>
        ///     可以穿透
        /// </summary>
        Penetrate
    }

    #endregion

    /// <summary>
    ///     常量定义类
    /// </summary>
    internal class SysDefine
    {
        //路径常量
        //public const string SYS_CANVAS_PATH = "UI/Canvas/Canvas";
        //public const string SYS_PATH_UIFORMS_CONFIG_INFO = "UIFormsConfigInfo";
        //public const string SYS_PATH_CONFIGINFO = "SysConfigInfo";
        /// <summary>
        ///     Canvas标签常量
        /// </summary>
        public const string SYS_CANVAS_TAG = "_TagCanvas";

        /// <summary>
        ///     正常窗体 节点常量
        /// </summary>
        public const string SYS_NORMAL_NODE = "Normal";

        /// <summary>
        ///     固定功能窗体节点常量名称
        /// </summary>
        public const string SYS_FIXED_NODE = "Fixed";

        /// <summary>
        ///     弹窗节点常量名称
        /// </summary>
        public const string SYS_POPOUP_NODE = "PopUp";

        /// <summary>
        ///     脚本模块管理节点常量名称
        /// </summary>
        public const string SYS_SCRIPTMANAGER_NODE = "_ScriptMgr";

        //摄像机层深常量
    }
}