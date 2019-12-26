/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：ResourceTable 资源配置表数据格式接口                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/


namespace SsitEngine.Unity.Resource.Data
{
    /// <summary>
    ///     资源配置表数据格式接口
    /// </summary>
    public interface IResourceData
    {
        /// <summary>
        ///     资源ID
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     资源名称（五个字以内）
        /// </summary>
        string Name { get; }


        /// <summary>
        ///     资源描述
        /// </summary>
        string Desc { get; }


        /// <summary>
        ///     资源类型
        ///     1.UI预设
        ///     2.图集/精灵
        ///     3.动态物体预设
        ///     4.音频
        ///     5.文本配置(Json)
        ///     6.其他
        /// </summary>
        int Type { get; }

        /// <summary>
        ///     资源AB包标识
        /// </summary>
        string BundleId { get; }

        /// <summary>
        ///     AB包中的资源名称
        /// </summary>
        string ResName { get; }

        /// <summary>
        ///     资源在resource目录下的层级目录
        /// </summary>
        string ResourcePath { get; }
    }
}