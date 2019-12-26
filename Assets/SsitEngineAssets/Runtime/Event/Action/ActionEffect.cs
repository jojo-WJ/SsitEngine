using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    public enum ENEffectType
    {
        Active,
        Emit
    }

    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionEffect : ActionBase
    {
        [Tooltip("特效对象")] public Transform mEffect;

        public ENEffectType mEffectType; //特效类型

        /*
         * 以下字段纯属现阶段模拟（由于时间原因无法进一步重构）
         */
        [Tooltip("特效生命周期")] public float mHealthDuration; //应急物资的容量  tip： -1 代表无穷大

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
        }
    }
}