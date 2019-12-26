using System.Collections.Generic;
using System.Linq;
using Framework.Config;
using Framework.Data;
using Framework.SsitInput;
using SSIT.proto;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using Table;
using UnityEngine;

namespace Framework.Logic
{
    /// <summary>
    /// 道具类型
    /// </summary>
    public enum EnItemType
    {
        ET_None = 0,

        /// <summary>
        /// 事故
        /// </summary>
        ET_Accident = 1,

        /// <summary>
        /// 普通类
        /// </summary>
        ET_Normal = 2,

        /// <summary>
        /// 任务类
        /// </summary>
        ET_Mission = 3,

        /// <summary>
        /// 角色模型类
        /// </summary>
        ET_Npc = 4,

        /// <summary>
        /// 后台编辑资源
        /// </summary>
        ET_EditRes = 5
    }

    /// <summary>
    /// 通用物体子类型
    /// </summary>
    public enum EnItNoSubType
    {
        ENS_None = 0,
        ENS_Common,
        ENS_ForceUint,
        ENS_MHQ,
        ENS_XFP,

        ENS_CommonIk
        /*
         1、通用类型 2、应急力量类型 3、阀门 4、泵 5、消防炮
         6、椅子 7、灭火器 8、门
        */
    }

    public enum ENNpcType
    {
        EN_None = 0,
        EN_Stretcher = 1,
        EN_Worker = 2,
        EN_Doctor = 3,
        EN_Nurse = 4,
        EN_NoRelated = 5,
        EN_OutContract = 6
    }

    /// <summary>
    /// 事故类型
    /// </summary>
    public enum ENAccidentType
    {
        AD_None = 0,

        /// <summary>
        /// 火焰
        /// </summary>
        AD_Fire = 1,

        /// <summary>
        /// 气体泄漏
        /// </summary>
        AD_Gas = 2,

        /// <summary>
        /// 伤亡
        /// </summary>
        AD_Hurt = 3,

        /// <summary>
        /// 障碍物
        /// </summary>
        AD_Obstacle = 4
    }

    public enum EnAssistType
    {
        AT_None = 0,

        //没用
        AT_Resolute,
        AT_Trigger,
        AT_Tag,
        AT_DrillRes
    }

    public class DataItemProxy : DataProxy<ItemTableModel>
    {
        public const int c_sTagItemId = 2205; //标签道具id
        public const int c_sClientTerminal = 79999; //客户端终端角色id

        //划线专属
        public const int c_sArrowItem = 2104; //箭头预设
        public const int c_scordonItem = 2103; //警戒线预设


        public const int c_sInterComId = 30003; //对讲机
        private Dictionary<string, ForceUintTableModel> mForceUintTable;
        private Dictionary<string, NpcTableModel> mNpcBaseTable;


        private Dictionary<string, PlayerTableModel> mPlayerBaseTable;

        public DataItemProxy( IDataManager dataManager ) : base(dataManager)
        {
        }


        public ItemTableModel ItemModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }

        public PlayerTableModel PlayerModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mPlayerBaseTable[index];
            }
        }

        public ForceUintTableModel ForceUintModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mForceUintTable[index];
            }
        }

        public NpcTableModel NpcModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mNpcBaseTable[index];
            }
        }

        #region 数据查询接口

        public List<ItemDefine> GetDataByType( EnItemType type )
        {
            return ItemModel.Item.Where(x => { return x.ItemType == (int) type; }).ToList();
        }

        public List<SkillDefine> GetItemSkillList( int itemID )
        {
            var itemDefine = GetData<ItemDefine>(itemID);
            if (itemDefine != null)
            {
                var SkillDefines = new List<SkillDefine>();
                foreach (var ss in itemDefine.AttachedSkillList)
                {
                    if (ss == 0) continue;
                    SkillDefines.Add(m_dataManager.GetData<SkillDefine>((int) EnTableType.SkillTable, ss));
                }
                return SkillDefines;
            }
            return null;
        }

        public List<SkillAttribute> GetItemSkillInfoList( int itemID )
        {
            var skillDefines = GetItemSkillList(itemID);

            if (skillDefines != null)
            {
                skillDefines = skillDefines.FindAll(x => { return x != null; });
                return skillDefines.ConvertAll(delegate( SkillDefine ss ) { return ss.Create<SkillAttribute>(ss.Id); });
            }
            return null;
        }

        public List<ItemDefine> GetCarryItemList( int itemID )
        {
            var itemDefine = GetModelData(itemID);
            if (itemDefine != null)
            {
                var ItemDefines = new List<ItemDefine>();
                foreach (var ss in itemDefine.AttachedItemList)
                {
                    if (ss == 0) continue;
                    ItemDefines.Add(GetData<ItemDefine>(ss));
                }
                return ItemDefines;
            }
            return null;
        }

        public List<ItemDefine> GetCarryItemList( ItemDefine define )
        {
            if (define != null)
            {
                var ItemDefines = new List<ItemDefine>();
                foreach (var ss in define.AttachedItemList)
                {
                    if (ss == 0) continue;
                    ItemDefines.Add(GetData<ItemDefine>(ss));
                }
                return ItemDefines;
            }
            return null;
        }

        public List<ItemDefine> GetCarryItemList( ForceUintDefine define )
        {
            if (define != null)
            {
                var ItemDefines = new List<ItemDefine>();
                foreach (var ss in define.AttachedItemList)
                {
                    if (ss == 0) continue;
                    ItemDefines.Add(GetData<ItemDefine>(ss));
                }
                return ItemDefines;
            }
            return null;
        }

        /// <summary>
        /// 根据所选类型，获取道具基本信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="itemSubType"></param>
        /// <returns>实际所需的单位信息列表</returns>
        /// <remarks>paramT:ForceUintDefine为载具列表/ paramT：ItemDefine 为通用信息</remarks>
        public List<T> GetUintList<T>( EnItemType type, int itemSubType ) where T : class
        {
            switch (type)
            {
                case EnItemType.ET_Normal:
                {
                    var normalSubType = (EnItNoSubType) itemSubType;
                    switch (normalSubType)
                    {
                        case EnItNoSubType.ENS_ForceUint:
                            return ForceUintModel.ForceUint as List<T>;
                        default:
                            return ItemModel.Item as List<T>;
                    }
                }
                default:
                    return ItemModel.Item as List<T>;
            }
        }

        /// <summary>
        /// 根据itemid获取modelid进行道具实例化
        /// </summary>
        /// <param name="itemId">道具id</param>
        /// <returns></returns>
        public ItemDefine GetModelData( int itemId )
        {
            var flag = itemId / 10000;
            if (flag == 5)
            {
                var define = ForceUintModel.GetForceUintByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找力量单位对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }

                return ItemModel.GetItemByItemID(define.MapId);
            }
            if (flag == 6 || flag == 7)
            {
                var define = PlayerModel.GetPlayerByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }
                return ItemModel.GetItemByItemID(define.MapId);
            }
            return ItemModel.GetItemByItemID(itemId);
        }

        public DataBase GetData( int itemId )
        {
            var flag = itemId / 10000;
            if (flag == 5)
            {
                var define = ForceUintModel.GetForceUintByID(itemId);
                if (define == null) SsitDebug.Error("查找力量单位对象不存在：ForceUintDefine：itemid" + itemId);

                return define;
            }
            if (flag == 6 || flag == 7)
            {
                var define = PlayerModel.GetPlayerByID(itemId);
                if (define == null) SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                return define;
            }
            return ItemModel.GetItemByItemID(itemId);
        }

        public ItemDefine GetItemData( int itemId )
        {
            var flag = itemId / 10000;
            var mapId = itemId;
            if (flag == 4)
            {
                var define = NpcModel.GetNpcByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找力量单位对象不存在：NpcDefine：itemid" + itemId);
                    return null;
                }
                mapId = define.MapId;
            }
            else if (flag == 5)
            {
                var define = ForceUintModel.GetForceUintByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找力量单位对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }
                mapId = define.MapId;
            }
            else if (flag == 6 || flag == 7)
            {
                var define = PlayerModel.GetPlayerByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }
                mapId = define.MapId;
            }
            return ItemModel.GetItemByItemID(mapId);
        }

        /// <summary>
        /// 获取显示信息
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ScenePrefabInfo GetPrefabInfoById( CharacterInfo info)
        {
            int itemId = 0;
            ItemDefine itemDefine = null;
            var flag = itemId / 10000;
            if (flag == 4)
            {
                var define = NpcModel.GetNpcByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找Npc单位对象不存在：NpcDefine：itemid" + itemId);
                    return null;
                }
                itemDefine = ItemModel.GetItemByItemID(define.MapId);
                return new ScenePrefabInfo
                {
                    ResoucesName = define.EditorPrefabPath,
                    ItemId = define.Id,
                    IconId = itemDefine.IconId
                };
            }

            if (flag == 5)
            {
                var define = ForceUintModel.GetForceUintByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找力量单位对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }
                itemDefine = ItemModel.GetItemByItemID(define.MapId);
                return new ScenePrefabInfo
                {
                    ResoucesName = itemDefine.ItemPrefabPath,
                    ItemId = define.Id,
                    IconId = itemDefine.IconId
                };
            }
            if (flag == 6 || flag == 7)
            {
                var define = PlayerModel.GetPlayerByID(itemId);
                if (define == null)
                {
                    SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                    return null;
                }
                itemDefine = ItemModel.GetItemByItemID(define.MapId);
                return new ScenePrefabInfo
                {
                    ResoucesName = define.EditorPrefabPath,
                    ItemId = define.Id,
                    IconId = itemDefine.IconId
                };
            }

            itemDefine = ItemModel.GetItemByItemID(itemId);

            return new ScenePrefabInfo
            {
                ResoucesName = itemDefine.ItemPrefabPath,
                ItemId = itemDefine.ItemID,
                IconId = itemDefine.IconId
            };
        }

        #endregion


        #region 继承

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        public override void OnRegister()
        {
            base.OnRegister();
            //mModelTable = LoadAndInitModel<ItemTableModel>(EnTableType.ItemTable.ToString(), true);
            //mPlayerBaseTable = LoadAndInitModel<PlayerTableModel>(EnTableType.PlayerTable.ToString(), true);
            //mForceUintTable = LoadAndInitModel<ForceUintTableModel>(EnTableType.ForceUintTable.ToString(), true);
            SyncLoadAndInitModel(EnTableType.ItemTable.ToString(), true);
            SyncLoadAndInitModel<PlayerTableModel>(EnTableType.PlayerTable.ToString(), true,
                list => mPlayerBaseTable = list);
            SyncLoadAndInitModel<ForceUintTableModel>(EnTableType.ForceUintTable.ToString(), true,
                list => mForceUintTable = list);
            SyncLoadAndInitModel<NpcTableModel>(EnTableType.NpcTable.ToString(), true,
                list => mNpcBaseTable = list);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            mModels = null;
            mPlayerBaseTable = null;
            mNpcBaseTable = null;
            mForceUintTable = null;
        }

        //查询对应表格的数据
        public override T GetData<T>( int itemId )
        {
            var flag = itemId / 10000;

            if (flag == 4)
            {
                var define = NpcModel.GetNpcByID(itemId);
                if (define == null) SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                return define as T;
            }
            if (flag == 5)
            {
                var define = ForceUintModel.GetForceUintByID(itemId);
                if (define == null) SsitDebug.Error("查找力量单位对象不存在：ForceUintDefine：itemid" + itemId);

                return define as T;
            }
            if (flag == 6 || flag == 7)
            {
                var define = PlayerModel.GetPlayerByID(itemId);
                if (define == null) SsitDebug.Error("查找人员对象不存在：ForceUintDefine：itemid" + itemId);
                return define as T;
            }
            return ItemModel.GetItemByItemID(itemId) as T;
        }

        #endregion
    }
}