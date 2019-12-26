using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    public class DataResourceProxy : DataProxy<ResourcesTableModel>
    {
        private List<ResourcesTableModel> m_resourcesModels;

        public DataResourceProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public ResourcesTableModel ResourcesModel => mModels[JsonLoader.DefaultComponeyName];

        public override void OnRegister()
        {
            base.OnRegister();
            //m_resourcesModels = LoadAndInitModel<ResourcesTableModel>(EnTableType.ResourcesTable.ToString());
            SyncLoadAndInitModel(EnTableType.ResourcesTable.ToString(), false);
        }

        public override void OnRemove()
        {
            m_resourcesModels = null;
            base.OnRemove();
        }

        public override void UpdateData( List<DataBase> dataList )
        {
            // TODO 该表
        }

        public override IResourceData GetData<IResourceData>( int id )
        {
            var resourcesDefine = ResourcesModel.GetResourcesById(id);
            if (resourcesDefine == null) return default;

            return resourcesDefine as IResourceData;
        }
    }
}