using Framework;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    public class ActionUI : ActionBase
    {
        [Tooltip("为true表示 on、off状态都会打开界面，否则off时关闭界面")]
        public bool m_mode = true;
        //  public En_UIForm uiForm = En_UIForm.EditorForm;

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            //bool state = m_actionParam.ParseByDefault(false);

            //var msgId = state ? UIMsg.OpenForm : UIMsg.CloseForm;
            //Facade.Instance.SendNotification((ushort)msgId, uiForm, data);

            var player = data as Player;
            //
            var state = (En_SwitchState) actionParam.ParseByDefault(0);
//                    case En_SwitchState.Off:
//                        var msgId = m_mode ? UIMsg.OpenForm : UIMsg.CloseForm;
//                        Facade.Instance.SendNotification((ushort)msgId, uiForm, data);
//                        break;
            if (player != null)
                switch (state)
                {
                    case En_SwitchState.On:
                    {
                        // Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, uiForm, data);
                        //一方面：这个时候可能网络端才同步过来权柄对象（在这块改之后，网络端装态为on的同步回调才执行，guid会被重置）
                        //另一方面data不能传null
                        (sender as BaseObject).OnChangeProperty(sender, EnPropertyId.Switch,
                            ((int) En_SwitchState.Off).ToString(), data);
                    }
                        break;
                }
        }
    }
}