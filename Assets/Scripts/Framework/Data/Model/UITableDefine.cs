using SsitEngine.Data;
using SsitEngine.Unity.Resource.Data;

namespace Table
{
    public partial class UIDefine : DataBase, IUIData
    {
        public int UILucencyType => uiLucencyType;

        public override int Id => id;

        public string Name => name;

        public string Desc => null;

        public int ResourceId => resourceId;

        public int UIShowMode => uiShowMode;

        public int UIShowType => uiShowType;

        public int GroupId => groupId;

        public override T Create<T>( int dataId )
        {
            return default;
        }

        public override void Apply( object obj )
        {
        }
    }
}