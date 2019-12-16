/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 14:02:53                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Data
{
    /// <summary>
    ///     数据模型的基类
    /// </summary>
    public abstract class ModelBase
    {
        /// <summary>
        ///     序列化列表
        /// </summary>
        /// <param name="ins">属性模型实例</param>
        public virtual void Deserialized( ModelBase ins )
        {
        }

        /// <summary>
        ///     序列化成字典
        /// </summary>
        /// <param name="ins">属性模型实例</param>
        public virtual void DeserializedRe( ModelBase ins )
        {
        }
    }
}