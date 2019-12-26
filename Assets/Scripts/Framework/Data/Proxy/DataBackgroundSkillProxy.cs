/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：后台技能代理                                                  
*│　作    者：qyy                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/17 11:11:57                             
*└──────────────────────────────────────────────────────────────┘
*/
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using System.Collections.Generic;
using Framework;
using Framework.Data;
using Framework.Logic;
using SsitEngine.Data;
using SsitEngine.Unity.Config;
using SsitEngine.Unity.Data;
using Table;

namespace SSIT.Framework
{
    /// <summary>
    /// 数据代理（可以处理多个数据模型）
    /// </summary>
	public class DataBackgroundSkillProxy : DataProxy<BackgroundSkillTableModel>
	{
		//todo:1写入自己代理的源数据模型列表（因其数据模型有多企业性质所以是个列表）
            
            
        //...

        //todo:2写入自己代理的源数据模型属性
        public BackgroundSkillTableModel BackgroundSkillTableModel
		{
			get
			{
				var index = ConfigManager.Instance.CompanyIndex;
				return mModels[index];
			}
		}

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dataManager"></param>
        /// <remarks>不想写静态的代理名称可以使用表名作为代理名称</remarks>
		public DataBackgroundSkillProxy(IDataManager dataManager) : base(dataManager/*,todo: EnTableType.Content.ToString()*/)
		{
		}

		#region 子类继承

        /// <summary>
        /// 更新数据（一般用于服务器的动态数据绑定）
        /// </summary>
        /// <param name="dataList"></param>
		public override void UpdateData(List<DataBase> dataList)
		{

		}

        /// <summary>
        /// 代理注册
        /// </summary>
		public override void OnRegister()
		{
			base.OnRegister();
            //todo:写入自己代理的源数据模型对象和表名--枚举名
            SyncLoadAndInitModel(EnTableType.BackgroundSkillTable.ToString(),true);
		}

        /// <summary>
        /// 代理移除
        /// </summary>
		public override void OnRemove()
		{
			base.OnRemove();
            //todo:清除自己代理的源数据模型对象和表名--枚举名
			mModels.Clear();
			mModels = null;
		}

		#endregion

		#region Internal Members

	    //todo:写自身代理的处理的数据逻辑

     
         public override T GetData<T>(int id)
         {
	         return BackgroundSkillTableModel.GetBackgroundSkillById(id) as T;
         }

         
		/// <summary>
		/// 根据技能类型获取技能
		/// </summary>
		/// <param name="skillTypeName"></param>
		/// <returns></returns>
         public List<BackgroundSkillDefine> GetDataBySkillType(string skillTypeName)
         {
	         return string.IsNullOrEmpty(skillTypeName)
		         ? null
		         : BackgroundSkillTableModel.BackgroundSkill.FindAll(x => x.skillType == skillTypeName);
         }
		
		
		/// <summary>
		/// 根据技能类型获取技能
		/// </summary>
		/// <param name="skillTypeName"></param>
		/// <returns></returns>
		public List<BackgroundSkillDefine> GetDataByID(int id)
		{
			return id==-1 ? BackgroundSkillTableModel.BackgroundSkill : BackgroundSkillTableModel.BackgroundSkill.FindAll(x => x.id == id);
		}

		

		/// <summary>
		/// 根据技能类型获取对应的SkillDefine
		/// </summary>
		/// <param name="skillTypeName"></param>
		/// <returns></returns>
		public List<SkillDefine> GetSkillsBySkillType(string skillTypeName)
		{			
			DataSkillProxy skillProxy = DataManager.Instance.GetDataProxy<DataSkillProxy>((int)EnLocalDataType.DATA_SKILL);
			if (skillTypeName=="全部")
			{
				return skillProxy.SkillModel.Skill;
			}
			var dataArray = GetDataBySkillType(skillTypeName);
			List<SkillDefine> skills = new List<SkillDefine>();
			foreach (var backgroundSkillDefine in dataArray)
			{
				SkillDefine define = skillProxy.GetData<SkillDefine>(backgroundSkillDefine.relationID);
				if (define!=null)
				{
					skills.Add(define);
				}
			}

			return skills;
		}
		
		/// <summary>
		/// 根据技能类型获取对应的SkillDefine
		/// </summary>
		/// <param name="skillTypeName"></param>
		/// <returns></returns>
		public List<SkillDefine> GetSkillsBySkillTypeID(int id)
		{
			DataSkillProxy skillProxy = DataManager.Instance.GetDataProxy<DataSkillProxy>((int)EnLocalDataType.DATA_SKILL);
			if (id==-1)
			{ //-1表示要获取全部技能信息
				return skillProxy.SkillModel.Skill;
			}
			
			var dataArray = GetDataByID(id);
			List<SkillDefine> skills = new List<SkillDefine>();
			foreach (var backgroundSkillDefine in dataArray)
			{
				SkillDefine define = skillProxy.GetData<SkillDefine>(backgroundSkillDefine.relationID);
				if (define!=null)
				{
					skills.Add(define);
				}
			}
			return skills;
		}



		#endregion
	}
}