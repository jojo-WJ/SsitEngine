/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/8 17:06:01                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.Data;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;

namespace Table
{
    public partial class PlayerDefine : DataBase
    {
        private string editorResourceName;
        private string resourceName;

        public override int Id => ID;

        public string ItemPrefabPath
        {
            get
            {
                if (resourceName == null)
                {
                    var item = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, MapId);
                    if (item == null)
                    {
                        SsitDebug.Error("后台人物资源模型配置异常");
                        return null;
                    }
                    var resInfo =
                        DataManager.Instance.GetData<ResourcesDefine>((int) EnLocalDataType.DATA_Res, item.ResId);

                    if (resInfo != null)
                        resourceName = resInfo.resourcePath;
                    else
                        SsitDebug.Error("资源模型配置异常");
                }
                return resourceName;
            }
        }

        public string EditorPrefabPath
        {
            get
            {
                if (editorResourceName == null)
                {
                    var resInfo =
                        DataManager.Instance.GetData<ResourcesDefine>((int) EnLocalDataType.DATA_Res, BackShowModelId);

                    if (resInfo != null)
                        editorResourceName = resInfo.resourcePath;
                    else
                        SsitDebug.Error("资源模型配置异常");
                }
                return editorResourceName;
            }
        }

        public override T Create<T>( int dataId )
        {
            var playerAttribute = new PlayerAttribute();
            playerAttribute.Profession = Profession;
            playerAttribute.Name = Name;
            var item = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, MapId);
            if (item != null)
            {
                playerAttribute.DataId = Id;

                playerAttribute.PrefabPath = item.ItemPrefabPath;
                //编辑模型获取
                playerAttribute.EditorPrefabPath = EditorPrefabPath;

                playerAttribute.Name = item.ItemName;
                playerAttribute.Description = item.ItemDesc;
                playerAttribute.Icon = item.IconId;
                playerAttribute.MapIconPath = item.ItemCircleIconPath;

                playerAttribute.ItemType = (EnItemType) item.ItemType;
                playerAttribute.OperateGroupType = (ENOpGroupType) item.OperateGroupType;
                playerAttribute.RelationType = (ENItemRelationType) item.RelationType;
                playerAttribute.RelationSkillList = item.RelationSkillList;

                playerAttribute.Profession = Profession;
                playerAttribute.SkillList = AttachedSkillList;
                playerAttribute.mEquipList = AttachedEquipList;
            }
            else
            {
                SsitDebug.Error("资源表格配置异常" + ID);
            }


            //...

            //npcAttribute.
            return playerAttribute as T;
        }

        public T CreateVariant<T>( int dataId ) where T : class
        {
            var playerAttribute = new NetPlayerAttribute();
            playerAttribute.Name = Name;
            var item = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, MapId);
            if (item != null)
            {
                playerAttribute.Name = item.ItemName;
                playerAttribute.PrefabPath = ItemPrefabPath;
                playerAttribute.Description = item.ItemDesc;
                playerAttribute.Icon = item.IconId;
                playerAttribute.MapIconPath = item.ItemCircleIconPath;

                playerAttribute.ItemType = (EnItemType) item.ItemType;
                playerAttribute.OperateGroupType = (ENOpGroupType) item.OperateGroupType;
                playerAttribute.RelationType = (ENItemRelationType) item.RelationType;
                playerAttribute.RelationSkillList = item.RelationSkillList;
                playerAttribute.SkillList = AttachedSkillList;

                //playerAttribute.Profession = Profession;
            }
            else
            {
                SsitDebug.Error("资源表格配置异常" + ID);
            }


            //...

            //npcAttribute.
            return playerAttribute as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}