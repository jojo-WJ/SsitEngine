using System.Collections.Generic;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionOnHit : ActionBase
    {
        [Header("被击行为回调")] public List<ActionEventParam> mActionEventParams;

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            for (var i = 0; i < mActionEventParams.Count; i++)
                mActionEventParams[i].mActionEvent.Invoke(sender, actionId, actionParam, data);
        }
    }
}