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
    public partial class NpcDefine : DataBase
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
            var npcAttribute = new NpcAttribute();
            npcAttribute.Profession = Profession;
            npcAttribute.Name = Name;
            var item = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, MapId);
            if (item != null)
            {
                npcAttribute.DataId = Id;

                npcAttribute.PrefabPath = item.ItemPrefabPath;
                //编辑模型获取
                npcAttribute.EditorPrefabPath = EditorPrefabPath;

                npcAttribute.Name = item.ItemName;
                npcAttribute.Description = item.ItemDesc;
                npcAttribute.Icon = item.IconId;
                npcAttribute.MapIconPath = item.ItemCircleIconPath;

                npcAttribute.ItemType = (EnItemType) item.ItemType;
                npcAttribute.OperateGroupType = (ENOpGroupType) item.OperateGroupType;
                npcAttribute.RelationType = (ENItemRelationType) item.RelationType;
                npcAttribute.RelationSkillList = item.RelationSkillList;

                npcAttribute.Profession = Profession;
                npcAttribute.SkillList = AttachedSkillList;
                npcAttribute.mEquipList = AttachedEquipList;
            }
            else
            {
                SsitDebug.Error("资源表格配置异常" + ID);
            }


            //...

            //npcAttribute.
            return npcAttribute as T;
        }

        public override void Apply( object obj )
        {
        }
    }
}