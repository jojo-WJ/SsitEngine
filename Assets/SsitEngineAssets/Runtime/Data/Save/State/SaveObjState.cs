using System;
using SsitEngine.DebugLog;
using SsitEngine.EzReplay;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// 场景物体缓存对象
/// </summary>
[Serializable]
public class SaveObjState : SavedBase
{
    //Track1:TransInfo
    public SerVector3 position;
    public SerQuaternion rotation;

    //Track2:BaseInfo
    public List<string> mBaseTrack;

    //Track3:AttributeInfo -- 子类进行扩充

    //通道1句柄缓存
    public bool isSkip1 = false;


    public SaveObjState()
    {

    }
    public SaveObjState(GameObject go)
    {
        if (go != null)
        {
            this.position = go.transform.position;
            this.rotation = go.transform.rotation;
            this.isActive = go.activeSelf;
        }
        else
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.isActive = false;
        }

    }

    //as this is not derived from MonoBehaviour, we have a constructor
    public void SetSaveObjState(GameObject go)
    {

        if (go != null)
        {
            this.position = go.transform.position;
            this.rotation = go.transform.rotation;
            this.isActive = go.activeSelf;
        }
        else
        {
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.isActive = false;
        }

    }


    #region 序列化

    //serialization constructor
    protected SaveObjState(SerializationInfo info, StreamingContext context)
    {
        isActive = info.GetBoolean("isActive");
        try
        {
            this.position = (SerVector3)info.GetValue("position", typeof(SerVector3));
            this.rotation = (SerQuaternion)info.GetValue("rotation", typeof(SerQuaternion));
        }
        catch
        {
            //not available if used an older version to save the replay, ignore
        }
        mBaseTrack = (List<string>)info.GetValue("bt", typeof(List<string>));

    }

    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("isActive", this.isActive);
        info.AddValue("position", position);
        info.AddValue("rotation", rotation);
        //Common deal
        info.AddValue("bt", this.mBaseTrack);

    }


    #endregion

    #region 子类实现

    /// <inheritdoc />
    public override bool IsDifferentTo(SavedBase state, Object2PropertiesMapping o2m)
    {
        var otherState = state as SaveObjState;
        if (otherState == null)
            return false;

        //active check
        bool changed = isActive != otherState.isActive;

        // check pos and rot
        if (!changed && position.isDifferentToByOffset(otherState.position, 1e-4f))
            changed = true;

        if (!changed && rotation.isDifferentToByOffset(otherState.rotation, 1e-4f))
            changed = true;
        

        return changed;
    }

    /// <inheritdoc />
    public override void SynchronizeProperties(GameObject go, Object2PropertiesMapping o2m, bool isReset,bool isFristFrame)
    {
        //HINT: lerping is still highly experimental
        if (go == null)
        {
            return;
        }

        go.SetActive(this.isActive);
        go.transform.position = this.position;
        go.transform.rotation = this.rotation;
    }


    /// <inheritdoc />
    public override void TrackClone(SavedBase otherState)
    {
        var cur = otherState as SaveObjState;
        if (cur == null)
        {
            SsitDebug.Error("缓存类型混乱");
            return;
        }

        mBaseTrack = cur.mBaseTrack;
    }
    #endregion


    #region Extension

    public override SavedBase Clone(bool isInit = true)
    {
        if (!isInit)
        {
            InitBaseTrack();
        }
        return new SaveObjState();
    }

    public virtual bool InitBaseTrack()
    {
        return false;
    }
    public virtual void ResetBaseTrack(List<string> baseTrack)
    {
       
    }

    #endregion



    #region IDispose

    #endregion

}
