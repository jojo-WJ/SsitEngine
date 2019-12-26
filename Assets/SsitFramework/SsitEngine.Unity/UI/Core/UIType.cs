/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UI面板类型                                                    
*│　作   者：Jusam                                       
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UI面板类型
    /// </summary>
    public class UIType
    {
        /// <summary>
        ///     是否清楚UI序列栈
        /// </summary>
        public bool IsClearStack = false;

        /// <summary>
        ///     UI透明度类型
        /// </summary>
        public UIFormLucencyType UIForm_LucencyType = UIFormLucencyType.Lucency;

        /// <summary>
        ///     UI面板显示模式
        /// </summary>
        public UIFormSHowMode UIForm_SHowMode = UIFormSHowMode.Normal;

        /// <summary>
        ///     UI面板属性类型
        /// </summary>
        public UIFormType UIForm_Type = UIFormType.Normal;
    }
}