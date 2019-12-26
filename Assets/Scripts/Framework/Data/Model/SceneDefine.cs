using Framework.Scene;
using SsitEngine.Data;
using UnityEditor;

namespace Table
{
    public partial class SceneDefine : DataBase, IInfoData
    {
        public override int Id => SceneID;

        public string ID => SceneID.ToString();

        public override T Create<T>( int dataId )
        {
            var level = new LevelAtrribute
            {
                SceneId = SceneID,
                SceneName = SceneFileName,
                SceneDisplayName = SceneName,
                ResourceName = SceneFileName
            };

            return level as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}