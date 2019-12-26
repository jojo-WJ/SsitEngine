/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 12:07:17                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Data;

namespace SsitEngine.Unity.Msic
{
    /// <summary>
    ///     场景镜像
    /// </summary>
    public class LevelResource : ResourceBase
    {
        /// <summary>
        ///     创建场景对象
        /// </summary>
        /// <param name="data"></param>
        public LevelResource( DataBase data )
        {
            Data = data;
        }

        public DataBase Data { get; set; }
    }
}