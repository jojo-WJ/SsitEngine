using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    public class DataCompanyProxy : DataProxy<CompanyTableModel>
    {
        public DataCompanyProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public CompanyTableModel CompanyModel => mModels[JsonLoader.DefaultComponeyName];

        #region 子类实现

        public override void OnRegister()
        {
            base.OnRegister();
            //mModelTable = LoadAndInitModel<CompanyTableModel>(EnTableType.CompanyTable.ToString());
            SyncLoadAndInitModel(EnTableType.CompanyTable.ToString(), false);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            mModels = null;
        }

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        public override T GetData<T>( int id )
        {
            return CompanyModel.GetCompanyByCompanyID(id) as T;
        }

        #endregion
    }
}