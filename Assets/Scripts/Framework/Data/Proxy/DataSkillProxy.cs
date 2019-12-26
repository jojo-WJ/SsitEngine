using System.Collections.Generic;
using Framework.Config;
using Framework.Data;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using Table;

namespace Framework.Logic
{
    public class DataSkillProxy : DataProxy<SkillTableModel>
    {
        /// <summary>
        /// 代理构造
        /// </summary>
        /// <param name="dataManager"></param>
        public DataSkillProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        /// <summary>
        /// 应急措施模型
        /// </summary>
        public SkillTableModel SkillModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }

        #region 数据查询接口

        /// <summary>
        /// 技能创建
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public SkillAttribute CreateSkill( int skillId )
        {
            var define = GetData<DataBase>(skillId);
            if (define == null)
            {
                SsitDebug.Error("技能合成失败" + skillId);
                return null;
            }

            return define.Create<SkillAttribute>(skillId);
        }

        #endregion

        #region 继承

        /// <summary>
        /// 代理注册
        /// </summary>
        public override void OnRegister()
        {
            base.OnRegister();
            //mModelTable = LoadAndInitModel<SkillTableModel>(EnTableType.SkillTable.ToString(),true);
            SyncLoadAndInitModel(EnTableType.SkillTable.ToString(), true);
        }

        /// <summary>
        /// 代理注销
        /// </summary>
        public override void OnRemove()
        {
            base.OnRemove();
            mModels = null;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="id">数据id</param>
        /// <returns>对应数据id的元数据</returns>
        public override T GetData<T>( int id )
        {
            return SkillModel.GetSkillBySkillID(id) as T;
        }

        /// <summary>
        /// 根据技能Id的链表添加对应的技能define
        /// </summary>
        /// <param name="skillIdList"></param>
        /// <returns></returns>
        public List<SkillDefine> GetSkillDefines(IEnumerable<int> skillIdList)
        {
            List<SkillDefine> skillDefines = new List<SkillDefine>();
            foreach (var skillId in skillIdList)
            {
              var skillDefine = GetData<SkillDefine>(skillId);
              if (skillDefine!=null)
              {
                  skillDefines.Add(skillDefine);
              }
            }

            return skillDefines;
        }


        /// <summary>
        /// 数据更新
        /// </summary>
        /// <param name="dataList"></param>
        public override void UpdateData( List<DataBase> dataList )
        {
        }

        #endregion
    }
}