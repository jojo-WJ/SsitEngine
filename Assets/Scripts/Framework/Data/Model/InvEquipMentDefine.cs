using SsitEngine.Data;
using SsitEngine.Unity.Avatar;

namespace Table
{
    public partial class InvEquipMentDefine : DataBase /*,todo:若存在自定义接口实现 ICustom*/
    {
        //todo:返回数据代理的唯一索引
        public override int Id => EqiupID;

        /// <summary>
        /// 元数据信息转属性信息
        /// </summary>
        /// <typeparam name="T">属性信息</typeparam>
        /// <param name="dataId"></param>
        /// <returns>IInfoData属性信息</returns>
        public override T Create<T>( int dataId )
        {
            var info = new ItemAtrribute();
            info.Id = EqiupID;
            info.Name = EquipName;
            info.Resources = PrefabPath;
            info.Slot = (InvSlot) SlotType;
            info.UseSlot = (InvUseSlot) UseSlotType;
            info.UseNodeType = (InvUseNodeType) UseNodeType;
            info.CombineIndex = IsCombine;

            //equipOffset
            //for (int i = 0; i < EquipOffset.Count; i++)
            //{
            //    string temp = EquipOffset[i];
            //    Debug.LogError("expression" + EquipOffset[i]);
            //    if (string.IsNullOrEmpty(temp))
            //    {
            //        continue;
            //    }

            //    string[] split = temp.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            //    if (split.Length < 2)
            //    {
            //        SsitDebug.Error("装备偏移配置异常");
            //    }

            //    int key = split[0].ParseByDefault(0);

            //    if (playerAttribute.mEquipOffsetMap.ContainsKey(key))
            //    {
            //        SsitDebug.Error($"装备偏移配置异常{i}");
            //    }
            //    else
            //    {
            //        Vector3 offset = split[1].ParseByDefault(Vector3.zero);
            //        playerAttribute.mEquipOffsetMap.Add(key, offset);
            //        Debug.LogError("expression" + key + offset);

            //    }

            //}
            return info as T;
        }

        /// <summary>
        /// 服务器数据的应用接口
        /// </summary>
        /// <param name="obj">应用对象</param>
        public override void Apply( object obj )
        {
        }
    }
}