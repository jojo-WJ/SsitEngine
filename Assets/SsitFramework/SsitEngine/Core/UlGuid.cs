/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 11:49:59                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core
{
    /// <summary>
    ///     唯一Id（2147483647）
    /// </summary>
    public class UlGuid
    {
        /// <summary>
        ///     唯一Id
        /// </summary>
        protected ulong mId;

        /// <summary>
        ///     构造Guid
        /// </summary>
        /// <param name="start"></param>
        public UlGuid( ulong start = 0 )
        {
            mId = start;
        }

        /// <summary>
        ///     此函数假定不能从多个线程创建新唯一Id
        /// </summary>
        /// <returns></returns>
        public ulong GenerateNewId()
        {
            return mId++;
        }
    }
}