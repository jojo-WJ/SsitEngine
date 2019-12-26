using System.Collections.Generic;
using Framework.Config;
using SsitEngine.Data;
using Table;

namespace Framework.Service
{
    public class DataSceneProxy : DataProxy<SceneTableModel>
    {
        private List<SceneTableModel> mSceneModelTable;

        public DataSceneProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public SceneTableModel SceneModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }


        #region 数据查询接口

        public List<SceneDefine> GetAllSceneDefine()
        {
            var allScene = new List<SceneDefine>();
            foreach (var model in mSceneModelTable) allScene.AddRange(model.Scene);
            return allScene;
        }

        /// <summary>
        /// 获取场景数据
        /// </summary>
        /// <param name="sceneId"></param>
        /// <returns></returns>
        public SceneDefine GetSceneTableData( int sceneId )
        {
            for (var i = 0; i < mSceneModelTable.Count; i++)
            {
                var define = mSceneModelTable[i].GetSceneBySceneID(sceneId);
                if (define != null) return define;
            }
            return null;
        }

        /// <summary>
        /// 获取场景数据
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns></returns>
        public SceneDefine GetSceneTableData( string sceneName )
        {
            return GetAllSceneDefine().Find(x => { return x.SceneFileName == sceneName; });
        }

        #endregion

        #region 继承

        public override T GetData<T>( int id )
        {
            return SceneModel.GetSceneBySceneID(id) as T;
        }

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        public override void OnRegister()
        {
            base.OnRegister();
            //mSceneModelTable = LoadAndInitModel<SceneTableModel>(EnTableType.SceneTable.ToString(), true);
            SyncLoadAndInitModel(EnTableType.SceneTable.ToString(), true);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            mSceneModelTable = null;
        }

        #endregion
    }
}