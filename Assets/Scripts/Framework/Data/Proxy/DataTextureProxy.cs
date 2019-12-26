using System;
using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using Table;

namespace SsitEngine.Unity.Data
{
    public class DataTextureProxy : DataProxy<TextureTableModel>
    {
        public DataTextureProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public TextureTableModel TextureTableModel => mModels[JsonLoader.DefaultComponeyName];


        public override void UpdateData( List<DataBase> dataList )
        {
            throw new NotImplementedException();
        }


        public override void OnRegister()
        {
            base.OnRegister();
            //m_textureTableModels = LoadAndInitModel<TextureTableModel>(EnTableType.TextureTable.ToString());
            SyncLoadAndInitModel(EnTableType.TextureTable.ToString(), false);
        }

        public override void OnRemove()
        {
            mModels.Clear();
            mModels = null;
            base.OnRemove();
        }

        public override T GetData<T>( int id )
        {
            var textureTableDefine = TextureTableModel.GetTextureById(id);
            if (textureTableDefine == null) return default;

            return textureTableDefine as T;
        }
    }
}