/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UITable 配置表格数据格式接口                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/


namespace SsitEngine.Unity.Resource.Data
{
    /// <summary>
    ///     配置表格数据格式接口
    /// </summary>
    public interface IUIData
    {
        /// <summary>
        ///     资源唯一标识
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     资源名称
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     资源描述
        /// </summary>
        string Desc { get; }

        /// <summary>
        ///     映射自资源表ResourceTable的唯一标识
        /// </summary>
        int ResourceId { get; }

        /// <summary>
        ///     UI显示模式
        /// </summary>
        int UIShowMode { get; }

        /// <summary>
        ///     UI显示类型
        /// </summary>
        int UIShowType { get; }

        /// <summary>
        /// UI透明类型
        /// </summary>
        // int UILucencyType { get; }
        /// <summary>
        ///     UI界面组标识
        ///     具有相同组标识的UI具有相同的显示隐藏时机
        /// </summary>
        int GroupId { get; }
    }
}