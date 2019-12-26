using SsitEngine.Data;
using SsitEngine.Unity.Resource.Data;

namespace Table
{
    public partial class AtlasDefine : DataBase, IAtlasData
    {
        public override int Id => id;
        public int ResourceId => resourceId;

        public override void Apply( object obj )
        {
        }

        public override T Create<T>( int dataId )
        {
            return default;
        }
    }
}