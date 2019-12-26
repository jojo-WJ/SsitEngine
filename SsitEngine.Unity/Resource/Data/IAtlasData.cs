namespace SsitEngine.Unity.Resource.Data
{
    /// <summary>
    ///     纹理配置表数据格式接口
    /// </summary>
    public interface IAtlasData
    {
        /// <summary>
        ///     唯一ID
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     映射自资源表ResourceTable的唯一标识
        /// </summary>
        int ResourceId { get; }
    }
}