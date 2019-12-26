/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/8 17:11:12                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.Data;
using Framework.Logic;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;
using EnItemType = Framework.EnItemType;

namespace Table
{
    public partial class ItemDefine : DataBase
    {
        private string resourceName;

        public override int Id => ItemID;

        public string ItemPrefabPath
        {
            get
            {
                if (resourceName == null)
                {
                    var resInfo =
                        DataManager.Instance.GetData<ResourcesDefine>((int) EnLocalDataType.DATA_Res, ResId);

                    if (resInfo != null)
                        resourceName = resInfo.resourcePath;
                    else
                        SsitDebug.Error($"资源模型配置异常{Id}");
                }
                return resourceName;
            }
        }

        public override T Create<T>( int id )
        {
            var itemType = (EnItemType) ItemType;

            BaseAtrribute info = null;
            
            if (info != null)
            {
                info.DataId = Id;
                info.Name = ItemName;
                info.PrefabPath = ItemPrefabPath;
                info.Description = ItemDesc;
                info.Icon = IconId;
                info.MapIconPath = ItemCircleIconPath;
                info.IsThrough = IsThrough == 0;
                info.PlaceLayer = PlaceLayer;
                info.ItemType = itemType;
                info.ItemSubType = itemSubType;
                info.OperateGroupType = (ENOpGroupType) OperateGroupType;
                info.RelationType = (ENItemRelationType) RelationType;
                info.RelationSkillList = RelationSkillList;
                info.SkillList = AttachedSkillList;
                info.ItemList = AttachedItemList;
                info.OperateGroupType = (ENOpGroupType) OperateGroupType;
            }

            return info as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}