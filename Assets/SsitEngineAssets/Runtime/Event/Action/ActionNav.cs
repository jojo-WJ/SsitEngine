using System.Collections.Generic;
using Framework;
using SsitEngine.Unity.Action;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 简单的UI交互行为
/// </summary>
public class ActionNav : ActionBase
{
    public bool isIk;

    [Tooltip("导航点")] [SerializeField] private Transform mNavPoint;

    public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
    {
        //bool state = m_actionParam.ParseByDefault(false);
        if (data is Player player)
        {
            var state = (En_SwitchState) actionParam.ParseByDefault(0);

            switch (state)
            {
                case En_SwitchState.On:
                {
                    if (isIk)
                        PreInteractive(player, mNavPoint, () =>
                        {
                            var obj = (BaseObject) sender;
                            var syncInfo = new List<PropertyParam>();
                            syncInfo.Add(new PropertyParam {property = EnPropertyId.Authority, param = player.Guid});
                            syncInfo.Add(new PropertyParam {property = EnPropertyId.SwitchIK, param = actionParam});

                            if (GlobalManager.Instance.IsSync)
                                obj.ChangeProperty(sender, syncInfo, data);
                            else
                                obj.OnChangeProperty(sender, syncInfo, data);
                        });
                }
                    break;
                case En_SwitchState.Off:
                {
                    if (isIk)
                        PreInteractive(player, mNavPoint, () =>
                        {
                            var obj = (BaseObject) sender;
                            var syncInfo = new List<PropertyParam>();
                            syncInfo.Add(new PropertyParam {property = EnPropertyId.SwitchIK, param = actionParam});
                            syncInfo.Add(new PropertyParam {property = EnPropertyId.Authority, param = string.Empty});

                            if (GlobalManager.Instance.IsSync)
                                obj.ChangeProperty(sender, syncInfo, data);
                            else
                                obj.OnChangeProperty(sender, syncInfo, data);
                        });
                }
                    break;
            }
        }
    }


    #region 弃用

    public void PreInteractive( Player curPlayer, Transform trigger, UnityAction readlyActionComplete = null )
    {
        var playerController = curPlayer.PlayerController;

        ////修正位置

        playerController.SetTargetPosition(trigger.position, false
            , () => { Debug.LogError("SetTargetPosition is interupt"); }
            , () =>
            {
                var rot = Quaternion.LookRotation(trigger.forward);
                var angle = new Vector3(playerController.transform.eulerAngles.x, rot.eulerAngles.y,
                    playerController.transform.eulerAngles.z);
                //var targetRotation = Quaternion.Euler(newPos);
                playerController.RotateToDir(angle, readlyActionComplete);
            });
    }

    #endregion
}