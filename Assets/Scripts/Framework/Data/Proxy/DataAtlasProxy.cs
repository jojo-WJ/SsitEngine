using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using Table;

namespace SsitEngine.Unity.Data
{
    public class DataAtlasProxy : DataProxy<AtlasTableModel>
    {
        public DataAtlasProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public AtlasTableModel TextureTableModel => mModels[JsonLoader.DefaultComponeyName];


        public override void UpdateData( List<DataBase> dataList )
        {
        }

        /// <summary>
        /// 代理注册
        /// </summary>
        public override void OnRegister()
        {
            base.OnRegister();
            SyncLoadAndInitModel(EnTableType.AtlasTable.ToString(), false);
        }

        public override void OnRemove()
        {
            mModels.Clear();
            mModels = null;
            base.OnRemove();
        }

        public override T GetData<T>( int id )
        {
            var textureTableDefine = TextureTableModel.GetAtlasById(id);
            if (textureTableDefine == null) return default;

            return textureTableDefine as T;
        }
    }
}