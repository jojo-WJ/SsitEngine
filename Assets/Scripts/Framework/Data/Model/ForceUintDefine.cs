/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/8 17:04:48                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.Data;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;

namespace Table
{
    public partial class ForceUintDefine : DataBase
    {
        public override int Id => ID;

        public override T Create<T>( int dataId )
        {
            var info = new VehicleAttribute();

            var item = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, MapId);
            if (item != null)
            {
                info.DataId = Id;
                var resInfo = DataManager.Instance.GetData<ResourcesDefine>((int) EnLocalDataType.DATA_Res, item.ResId);
                if (resInfo != null)
                    info.PrefabPath = resInfo.resourcePath;
                else
                    SsitDebug.Error("资源模型配置异常");

                info.Name = item.ItemName;
                info.Description = item.ItemDesc;
                info.Icon = item.IconId;
                info.MapIconPath = item.ItemCircleIconPath;
                info.ItemType = (EnItemType) item.ItemType;
                info.OperateGroupType = (ENOpGroupType) item.OperateGroupType;
                info.RelationType = (ENItemRelationType) item.RelationType;
                info.RelationSkillList = item.RelationSkillList;

                info.profession = Profession;
                info.SkillList = AttachedSkillList;
                info.itemList = AttachedItemList;
                info.playerList = PlayerList;
            }
            else
            {
                SsitDebug.Error("资源表格配置异常" + ID);
            }
            return info as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}