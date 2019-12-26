using UnityEngine;

namespace Framework.Data
{
    public interface ISave
    {
        /// <summary>
        /// 是否初始化
        /// </summary>
        //bool InitSave { get; set; }

        string Guid { get; set; }

        int ItemID { get; set; }


        /// <summary>
        /// 构造缓存对象
        /// </summary>
        /// <returns></returns>
        SavedBase GeneralSaveData(bool isDeepClone = false);

        /// <summary>
        /// 获取缓存的实体表现
        /// </summary>
        /// <returns></returns>
        GameObject GetRepresent();

        /// <summary>
        /// 同步序列化属性
        /// </summary>
        /// <param name="savedState">缓存数据</param>
        /// <param name="isReset">是否存在跳过</param>
        void SynchronizeProperties(SavedBase savedState, bool isReset, bool isFristFrame);

        /// <summary>
        /// 保存记录
        /// </summary>
        void SaveRecord();
    }
}