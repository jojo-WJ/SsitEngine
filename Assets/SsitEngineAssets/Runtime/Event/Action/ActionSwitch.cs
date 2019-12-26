using System;
using System.Collections.Generic;
using Framework;
using Framework.SceneObject;
using Framework.SceneObject.Trigger;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.Serialization;

namespace SsitEngine.Unity.Action
{
    [Serializable]
    public class ActionEventParam
    {
        [FormerlySerializedAs("onAction")] 
        public ActionEvent mActionEvent;

        public En_SwitchState state;
    }

    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionSwitch : ActionBase
    {
        [Header("行为回调")] public List<ActionEventParam> mActionEventParams;

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            var state = (En_SwitchState) actionParam.ParseByDefault(0);

            for (var i = 0; i < mActionEventParams.Count; i++)
                if (mActionEventParams[i].state == state)
                    mActionEventParams[i].mActionEvent.Invoke(sender, actionId, actionParam, data);
        }
    }
}