using SsitEngine.EzReplay;
using System;
using System.Runtime.Serialization;
using UnityEngine;


[Serializable()]
public class SaveProxyState : SavedBase, ISerializable
{

    public SaveProxyState()
    {

    }

    /// <summary>
    /// serialization constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected SaveProxyState(SerializationInfo info, StreamingContext context)
    {

    }



    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {

    }

    public override bool IsDifferentTo(SavedBase otherState, Object2PropertiesMapping o2m)
    {
        bool changed = false;

        // check infoData
        //if (!changed && o2m.sceneInstance != null && o2m.sceneInstance)
        {
            //if (o2m.sceneInstance.IsDifferentTo( this, otherState ))
            //{
            //    // 属性不同
            //    changed = true;
            //}
            //else
            //{
            //    // 属性相同的情况
            //    isSkip = true;
            //}
        }
        return changed;
    }

    //called to synchronize gameObjectClone of Object2PropertiesMapping back to this saved state
    public override void SynchronizeProperties(GameObject go, Object2PropertiesMapping o2m, bool isReset ,bool isFristFrame)
    {

        //HINT: lerping is still highly experimental
        if (go == null)
        {
            return;
        }
    }

}