using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using Table;

// ReSharper disable All

namespace Framework.Logic
{
    public class DataUIProxy : DataProxy<UITableModel>
    {
        public DataUIProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public UITableModel UITableModel
        {
            get { return mModels[JsonLoader.DefaultComponeyName]; }
        }


        public override void UpdateData( List<DataBase> dataList )
        {
        }

        public override void OnRegister()
        {
            base.OnRegister();
            //m_uiTableModels = LoadAndInitModel<UITableModel>(EnTableType.UITable.ToString());
            SyncLoadAndInitModel(EnTableType.UITable.ToString(), false);
        }

        public override void OnRemove()
        {
            mModels.Clear();
            mModels = null;
            base.OnRemove();
        }

        public override IUIData GetData<IUIData>( int id )
        {
            UIDefine resourcesDefine = UITableModel.GetUIById(id);
            if (resourcesDefine == null)
            {
                return default(IUIData);
            }

            return resourcesDefine as IUIData;
        }
    }
}