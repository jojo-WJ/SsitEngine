/**
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 20:13:36                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Data;

namespace Table
{
    public partial class ContentDefine : DataBase
    {
        public override int Id => index;

        public override T Create<T>( int dataId )
        {
            return default;
        }

        public override void Apply( object obj )
        {
        }
    }
}