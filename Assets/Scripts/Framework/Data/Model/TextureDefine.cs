using SsitEngine.Data;
using SsitEngine.Unity.Resource.Data;

namespace Table
{
    public partial class TextureDefine : DataBase, ITextureData
    {
        public override int Id => id;

        public int ResourceId => resourceId;

        public int Type => type;

        public int AtlasId => atlasId;


        public override void Apply( object obj )
        {
        }

        public override T Create<T>( int dataId )
        {
            return default;
        }
    }
}