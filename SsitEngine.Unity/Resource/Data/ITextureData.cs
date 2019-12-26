/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：TextureTable 纹理配置表数据格式接口                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/


namespace SsitEngine.Unity.Resource.Data
{
    /// <summary>
    ///     纹理配置表数据格式接口
    /// </summary>
    public interface ITextureData
    {
        /// <summary>
        ///     唯一ID
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     映射自资源表ResourceTable的唯一标识
        /// </summary>
        int ResourceId { get; }

        /// <summary>
        ///     纹理类型
        /// </summary>
        int Type { get; }

        /// <summary>
        ///     纹理尺寸 x|y
        /// </summary>
        int AtlasId { get; }
    }
}