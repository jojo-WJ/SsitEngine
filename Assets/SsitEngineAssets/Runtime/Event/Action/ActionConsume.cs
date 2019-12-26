using System.Globalization;
using Framework;
using Mirror;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionConsume : ActionBase
    {
        public float consumeThread; //每秒回复的阈值(如果想要更好的模拟火焰的扩散：应该考虑多方因素（可燃物、助燃点、着火点）)

        //[Header("回复行为回调")]
        //public List<ActionEventParam> mActionEventParams;

        public bool isServerOnly;
        public float MaxConsume; //回复上限

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (sender is BaseObject obj)
            {
                if (isServerOnly)
                {
                    if (NetworkServer.active)
                    {
                        //属性改变计算
                        var consume = actionParam.ParseByDefault(0.0f);
                        var curConsume = OnCalcConsume(obj, consume);

                        obj.OnChangeProperty(sender, EnPropertyId.OnConsume,
                            curConsume.ToString(CultureInfo.InvariantCulture), data);
                    }
                }
                else
                {
                    //单机行为

                    //属性改变计算
                    var consume = actionParam.ParseByDefault(0.0f);
                    var curConsume = OnCalcConsume(obj, consume);

                    obj.OnChangeProperty(sender, EnPropertyId.OnConsume,
                        curConsume.ToString(CultureInfo.InvariantCulture), data);
                }
            }


            //for (int i = 0; i < mActionEventParams.Count; i++)
            //{
            //    mActionEventParams[i].mActionEvent.Invoke(sender, actionId, actionParam, data);
            //}
        }

        private float OnCalcConsume( BaseObject obj, float attachValue )
        {
            //获取当前的损耗程度
            var currentLoss = obj.GetAttribute()[En_SceneObjectExParam.En_Health].ParseByDefault(0.0f);

            //获取
            return currentLoss + consumeThread + attachValue;
        }
    }
}