using System;
using System.Runtime.Serialization;
using SsitEngine;
using SsitEngine.EzReplay;
using UnityEngine;
/// <summary>
/// This class represents a state of a single object in one single frame. 
/// </summary>
[Serializable]
public class SavedBase :AllocatedObject, ISerializable/*, IDisposable*/
{
    public bool isActive = false;

    /// <summary>
    /// 属性改变检测句柄
    /// </summary>
    protected bool isChange;

    /// <summary>
    /// 属性改变检测句柄
    /// </summary>
    public bool IsChange
    {
        get { return isChange; }
        set { isChange = value; }
    }


    public SavedBase()
    {
        isChange = false;
    }

    #region 序列化

    //serialization constructor
    public SavedBase(SerializationInfo info, StreamingContext context)
    {

    }

  

    //serialization parse
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {

    }

    #endregion

    #region Core
    
    /// <summary>
    /// 通道句柄检测
    /// </summary>
    /// <param name="otherState">指定的属性通道</param>
    /// <param name="o2m">映射对象</param>
    /// <returns>是否写入回放文件</returns>
    public virtual bool IsDifferentTo(SavedBase otherState, Object2PropertiesMapping o2m)
    {
        return isChange;
    }

    /// <summary>
    /// 属性同步
    /// </summary>
    /// <param name="go">映射对象</param>
    /// <param name="o2m">属性映射对象</param>
    /// <param name="isReset">是否跳段</param>
    /// <param name="isFristFrame"></param>
    public virtual void SynchronizeProperties(GameObject go, Object2PropertiesMapping o2m, bool isReset, bool isFristFrame)
    {

    }

    /// <summary>
    /// 通道克隆
    /// </summary>
    /// <param name="otherState">指定的属性通道</param>
    public virtual void TrackClone(SavedBase otherState)
    {

    }

    public virtual SavedBase Clone(bool isInit = true)
    {
        return new SavedBase(); //SerializationUtils.Clone<SavedBase>(this);
    }


    //非托管资源释放在内存中存在时间会放长
    public override void Shutdown()
    {

    }
    #endregion

}
