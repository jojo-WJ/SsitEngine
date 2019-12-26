using SsitEngine.Data;
using SsitEngine.Unity.Resource.Data;

namespace Table
{
    public partial class ResourcesDefine : DataBase, IResourceData
    {
        public override int Id => id;

        public string Name => name;

        public string Desc => desc;

        public int Type => type;

        public string BundleId => bundleId;

        public string ResName => resName;

        public string ResourcePath => resourcePath;

        public override T Create<T>( int dataId )
        {
            return default;
        }

        public override void Apply( object obj )
        {
        }
    }
}