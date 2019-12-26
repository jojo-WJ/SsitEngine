using System.Collections.Generic;
using Framework;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为ActionActiveHelper
    /// </summary>
    public class ActionActivehaltsHelper : ActionBase
    {
        [SerializeField] private List<ActiveParam> m_activeParams;

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            var state = (En_SwitchState) actionParam.ParseByDefault(0);


            for (var i = 0; i < m_activeParams.Count; i++)
            {
                var param = m_activeParams[i];

                if (param.state == state)
                {
                    for (var j = 0; j < param.activeObjs.Length; j++)
                        if (param.activeObjs[j])
                        {
                            param.activeObjs[j].SetActive(false);
                            param.activeObjs[j].SetActive(true);
                        }

                    for (var j = 0; j < param.deactiveObjs.Length; j++)
                        if (param.deactiveObjs[j])
                            param.deactiveObjs[j].SetActive(false);
                }
            }
        }
    }
}