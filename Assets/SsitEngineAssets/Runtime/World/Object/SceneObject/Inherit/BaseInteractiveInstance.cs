using Framework.SsitInput;
using RootMotion.FinalIK;
using SsitEngine.EzReplay;
using SsitEngine.QuestManager;
using SsitEngine.Unity.SceneObject;
using System;
using System.Collections.Generic;
using Framework.Data;
using Framework.Quest;
using Framework.Utility;
using Packages.Rider.Editor.Util;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.Events;
using QuestHelper = Framework.Quest.QuestHelper;

namespace Framework.SceneObject
{
    public enum OperatorObjectType
    {
        None,
        XFP,
        PumpSwich,
        MHX,
        MHQ,
        Valve,
        SIREN_HAND,
        CHAIR,
    }

    [System.Serializable]
    public class InteractionParam
    {
        public InteractionObject interactionObject;

        public List<FullBodyBipedEffector> effectors;
    }
    
    public class BaseInteractiveInstance : BaseSceneInstance, IOnOff, ITriggle
    {
        protected bool m_IsOn;

        protected bool isInit = false;


        public virtual void SetOff( BaseSceneInstance triggler )
        {
        }

        public virtual void SetOn( BaseSceneInstance triggler )
        {
        }

        public virtual void SetOn( bool isOn, BaseSceneInstance triggler )
        {
            if (triggler && triggler.HasAuthority)
            {
                //access:任务系统接入
                //Facade.Instance.SendMessage(En_QuestsMsg.En_Interactive.ToString(), null, triggler.GroupUID, QuestParamGenerator.GeneratorParam(En_QuestsMsg.En_Interactive, isOn, this.ObjectGUID), triggler.ObjectGUID);
                //Facade.Instance.SendMessage(En_QuestsMsg.En_Technology.ToString(), null, triggler.GroupUID, QuestParamGenerator.GeneratorParam(En_QuestsMsg.En_Technology, isOn, this.ObjectGUID), triggler.ObjectGUID);
            }
        }


        public virtual string GetTriggler()
        {
            if (GlobalManager.Instance.CachePlayer != null)
            {
                return GlobalManager.Instance.CachePlayer.Guid;
            }
            return null;
        }

        //IK
        protected void PlayerIkAnimation( BaseInteractiveInstance baseInteractiveInstance, BaseSceneInstance player,
            UnityAction completeCallBack = null )
        {
            //前往触发点
            PlayerInstance curPlayer = player as PlayerInstance;
            if (curPlayer == null)
            {
                return;
            }
            InputManager.Instance.LockInput = true;
            //curPlayer.thirdPersonManipulator.SetTargetPosition(baseInteractiveObject.IkTriggler.position
            //    , () =>
            //    {
            //        curPlayer.State = EN_CharacterActionState.EN_CHA_Stay;
            //        InputManager.Instance.LockInput = false;
            //    }
            //    , () => { } //MainProcess.inputStateManager.operatorManipulator.PreInteractive(curPlayer, IkTriggler, completeCallBack); }
            //    );
        }


        #region New Version

        public bool isIk = false;
        public bool isRange = false;
        public bool isSync = true;

        [Tooltip("开绑定的IK类型")] public List<InteractionParam> mForwardInteractionParams;

        [Tooltip("关绑定的IK类型")] public List<InteractionParam> mBackwardInteractionParams;

        [Tooltip("IK事件权重类型")] public FullBodyBipedEffector mForwardCheckEffector;
        public FullBodyBipedEffector mBackwardCheckEffector;

        public bool isPickUp;

        public bool isPause;

        public bool isStop;

        public bool isEvent;


        [Header("交互道具初始化属性（可配置）")] [Space(6)] public En_SwitchState m_state = En_SwitchState.Off;
        protected HashSet<PlayerInstance> m_TriggerList = new HashSet<PlayerInstance>();

        public Transform IkTriggler { get; protected set; }

        protected virtual void Awake()
        {
            SphereCollider sc = GetComponentInChildren<SphereCollider>(true);
            if (sc == null)
            {
                return;
            }
            IkTriggler = sc.transform;
        }

        public virtual En_SwitchState GetSwitchState()
        {
            return AttachAttribute
                ? AttachAttribute.mState
                : (En_SwitchState) LinkObject.GetProperty(EnPropertyId.Switch).ParseByDefault(-1);
        }

        /// <summary>
        /// 属性改变回调
        /// </summary>
        /// <param name="propertyId">属性id</param>
        /// <param name="property">属性参数(字符串化)</param>
        /// <param name="data">自定义属性数据</param>
        public override void OnChangePropertyCallback( EnPropertyId propertyId, string property, object data = null )
        {
            base.OnChangePropertyCallback(propertyId, property, data);

            switch (propertyId)
            {
                case EnPropertyId.Switch:
                {
                    if (isRange)
                    {
                        PlayIK(LinkObject, propertyId, property, data);
                    }
                    else if (isSync)
                    {
                        Play(LinkObject, propertyId, property, data);
                    }
                    else
                    {
                        OnPlay(LinkObject, propertyId, property, data);
                    }
                }
                    break;
                case EnPropertyId.SwitchIK:
                {
                    OnPlayIK(LinkObject, propertyId, property, data);
                }
                    break;
                case EnPropertyId.OnSwitch:
                    OnSwitch(LinkObject, propertyId, property, data);
                    break;
            }
        }

        /// <summary>
        /// 操作端执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionId"></param>
        /// <param name="actionParam"></param>
        /// <param name="data"></param>
        protected void Play( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            //交互预处理
            if (data is Player player)
            {
                En_SwitchState state = (En_SwitchState) actionParam.ParseByDefault(0);

                switch (state)
                {
                    case En_SwitchState.Off:
                    {
                        if (LinkObject.GetParent() == null)
                            LinkObject.SetParent(player.Guid);

                        List<PropertyParam> syncInfo = new List<PropertyParam>();
                        syncInfo.Add(new PropertyParam() {property = EnPropertyId.OnSwitch, param = actionParam});
                        syncInfo.Add(new PropertyParam() {property = EnPropertyId.Authority, param = string.Empty});

                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, syncInfo, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, syncInfo, data);
                        }
                    }
                        break;

                    case En_SwitchState.On:
                    {
                        List<PropertyParam> syncInfo = new List<PropertyParam>();
                        syncInfo.Add(new PropertyParam() {property = EnPropertyId.Authority, param = player.Guid});
                        syncInfo.Add(new PropertyParam() {property = EnPropertyId.OnSwitch, param = actionParam});

                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, syncInfo, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, syncInfo, data);
                        }
                    }
                        break;
                    case En_SwitchState.Begin:
                    {
                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                        }
                    }
                        break;
                    case En_SwitchState.End:
                    {
                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                        }
                    }
                        break;
                }
            }
        }

        /// <summary>
        /// 操作端执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionId"></param>
        /// <param name="actionParam"></param>
        /// <param name="data"></param>
        protected void OnPlay( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            //交互预处理
            if (data is Player player)
            {
                En_SwitchState state = (En_SwitchState) actionParam.ParseByDefault(0);

                switch (state)
                {
                    case En_SwitchState.Off:
                    {
                        if (LinkObject.GetParent() == null)
                            LinkObject.SetParent(player.Guid);
                        LinkObject.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                        LinkObject.ChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                    }
                        break;
                    case En_SwitchState.On:
                    {
                        LinkObject.ChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                        LinkObject.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                    }
                        break;
                    case En_SwitchState.Begin:
                    {
                        LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                    }
                        break;
                    case En_SwitchState.End:
                    {
                        LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                    }
                        break;
                }
            }
        }

        /// <summary>
        /// 交互端执行
        /// </summary>
        protected void PlayIK( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            //交互预处理
            if (data is Player player)
            {
                En_SwitchState state = (En_SwitchState) actionParam.ParseByDefault(0);
                var obj = (BaseObject) sender;

                switch (state)
                {
                    case En_SwitchState.On:
                    {
                        PreInteractive(player, IkTriggler,
                            () =>
                            {
                                if (!isSync)
                                {
                                    OnPlay(LinkObject, actionId, actionParam, data);
                                    return;
                                }
                                if (LinkObject.GetParent() != null)
                                    return;
                                List<PropertyParam> syncInfo = new List<PropertyParam>();
                                syncInfo.Add(new PropertyParam()
                                    {property = EnPropertyId.Authority, param = player.Guid});
                                syncInfo.Add(
                                    new PropertyParam() {property = EnPropertyId.SwitchIK, param = actionParam});

                                obj.ChangeProperty(sender, syncInfo, data);
                            },
                            () =>
                            {
                                obj.ChangeProperty(sender, EnPropertyId.Switch, ((int) En_SwitchState.Off).ToString());
                            });
                    }
                        break;
                    case En_SwitchState.Off:
                    {
                        PreInteractive(player, IkTriggler, () =>
                            {
                                if (!isSync)
                                {
                                    OnPlay(LinkObject, actionId, actionParam, data);
                                    return;
                                }
                                List<PropertyParam> syncInfo = new List<PropertyParam>();

                                if (LinkObject.GetParent() == null)
                                {
                                    syncInfo.Add(new PropertyParam()
                                        {property = EnPropertyId.Authority, param = player.Guid});
                                    //LinkObject.SetParent(player.Guid);
                                }
                                syncInfo.Add(
                                    new PropertyParam() {property = EnPropertyId.SwitchIK, param = actionParam});
                                syncInfo.Add(new PropertyParam()
                                    {property = EnPropertyId.Authority, param = String.Empty});

                                obj.ChangeProperty(sender, syncInfo, data);
                            },
                            () =>
                            {
                                obj.ChangeProperty(sender, EnPropertyId.Switch, ((int) En_SwitchState.On).ToString());
                            }
                        );
                    }
                        break;
                    case En_SwitchState.Begin:
                    {
                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, player.Guid, data);
                        }
                    }
                        break;
                    case En_SwitchState.End:
                    {
                        if (GlobalManager.Instance.IsSync)
                        {
                            LinkObject.ChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                        }
                        else
                        {
                            LinkObject.OnChangeProperty(sender, EnPropertyId.Authority, String.Empty, data);
                        }
                    }
                        break;
                }
            }
        }

        /// <summary>
        /// 交互执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionId"></param>
        /// <param name="actionParam"></param>
        /// <param name="data"></param>
        protected void OnPlayIK( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (data is Player player)
            {
                En_SwitchState state = (En_SwitchState) actionParam.ParseByDefault(0);
                //PreInteractive(player,trigger)
                var obj = (BaseObject) sender;
                if (!isIk)
                {
                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                    return;
                }

                var playerController = player.PlayerController;
                if (state == En_SwitchState.On)
                {
                    // 注册IK回调
                    if (isPickUp)
                    {
                        playerController.IkSystem.OnInteractionPickUp =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject ) =>
                            {
                                if (effectorType == mForwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                }
                            };
                    }

                    if (!isPause && isStop)
                    {
                        playerController.IkSystem.OnInteractionStop =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject ) =>
                            {
                                if (effectorType == mForwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //停止交互行为
                                    if (playerController.syncCom.hasAuthority)
                                    {
                                        playerController.LockInput = false;
                                    }
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                }
                            };
                    }

                    if (isPause)
                    {
                        playerController.IkSystem.OnInteractionPause =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject ) =>
                            {
                                if (effectorType == mForwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //停止交互行为
                                    if (playerController.syncCom.hasAuthority)
                                    {
                                        playerController.LockInput = false;
                                    }
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                    //playerController.IkSystem.ResetAllCallBack();
                                }
                            };
                    }

                    if (isEvent)
                    {
                        playerController.IkSystem.OnInteractionEvent =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject,
                                InteractionObject.InteractionEvent interactionEvent ) =>
                            {
                                if (effectorType == mForwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                }
                            };
                    }

                    OnInteractive(player, playerController, mForwardInteractionParams);
                }
                else if (state == En_SwitchState.Off)
                {
                    /*if (mBackwardInteractionParams.Count <= 0)
                    {
                        obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                        return;
                    }*/

                    if (isPause && isStop)
                    {
                        playerController.IkSystem.OnInteractionStop =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject ) =>
                            {
                                if (effectorType == mForwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //停止交互行为
                                    if (playerController.syncCom.hasAuthority)
                                    {
                                        playerController.LockInput = false;
                                    }
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                }
                            };
                    }

                    if (isPause)
                    {
                        OnInteractive(player, playerController, mForwardInteractionParams, true);
                    }
                    else if (isEvent)
                    {
                        playerController.IkSystem.OnInteractionEvent =
                            ( FullBodyBipedEffector effectorType, InteractionObject interactionObject,
                                InteractionObject.InteractionEvent interactionEvent ) =>
                            {
                                if (effectorType == mBackwardCheckEffector && interactionObject.name == this.name)
                                {
                                    //var obj = (BaseObject)sender;
                                    obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                                }
                            };
                        OnInteractive(player, playerController, mBackwardInteractionParams);
                    }
                    else
                    {
                        //var obj = (BaseObject)sender;
                        obj.OnChangeProperty(sender, EnPropertyId.OnSwitch, actionParam, data);
                    }
                }
            }
        }

        /// <summary>
        /// 真正交互执行（同步执行）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionId"></param>
        /// <param name="actionParam"></param>
        /// <param name="data"></param>
        protected virtual void OnSwitch( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            En_SwitchState state = (En_SwitchState) actionParam.ParseByDefault(0);

            if (data is Player player && player.SceneInstance.HasAuthority)
            {
                InputManager.Instance.LockInput = false;
            }

            if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
                return;

            Player tmpPlayer = data as Player;
            string playerGuid = tmpPlayer != null ? tmpPlayer.Guid : string.Empty;
            switch (state)
            {
                case En_SwitchState.On:
                    IkTriggler?.gameObject.SetActive(false);
                    Facade.Instance.SendNotification((ushort) En_QuestsMsg.En_Interactive,
                        new QuestMessageArgs(0, this, playerGuid,
                            QuestHelper.GeneratorParam(En_QuestsMsg.En_Interactive, new object[] {true, Guid})));
                    break;
                case En_SwitchState.Off:
                    IkTriggler?.gameObject.SetActive(true);
                    Facade.Instance.SendNotification((ushort) En_QuestsMsg.En_Interactive,
                        new QuestMessageArgs(0, this, playerGuid,
                            QuestHelper.GeneratorParam(En_QuestsMsg.En_Interactive, new object[] {false, Guid})));
                    break;
            }
            m_TriggerList.Clear();
        }

        /// <summary>
        /// 发射特效
        /// </summary>
        protected virtual void EmitEffect( bool active )
        {
        }


        /// <summary>
        /// IK预交互状态
        /// </summary>
        /// <param name="curPlayer"></param>
        /// <param name="trigger"></param>
        /// <param name="readlyActionComplete"></param>
        protected virtual void PreInteractive( Player curPlayer, Transform trigger,
            UnityAction readlyActionComplete = null, UnityAction readlyActionCancle = null )
        {
            var playerController = curPlayer.PlayerController;

            ////修正位置
            playerController.SetTargetPosition(trigger.position, false
                , () =>
                {
                    Debug.LogError("SetTargetPosition is interupt");
                    readlyActionCancle?.Invoke();
                }
                , () =>
                {
                    //Quaternion rot = Quaternion.LookRotation(trigger.forward);
                    //var angle = new Vector3(playerController.transform.eulerAngles.x, rot.eulerAngles.y, playerController.transform.eulerAngles.z);
                    //var targetRotation = Quaternion.Euler(newPos);
                    playerController.RotateToDir(trigger.forward, readlyActionComplete);
                });
        }

        public void OnInteractive( Player player, ThirdPersonController curPlayer, List<InteractionParam> interactions,
            bool isResume = false )
        {
            if (interactions.Count > 0)
            {
                if (player.SceneInstance.HasAuthority)
                {
                    InputManager.Instance.LockInput = true;
                }
            }

            for (int i = 0; i < interactions.Count; i++)
            {
                var interaction = interactions[i];

                var effectors = interaction.effectors;

                for (int j = 0; j < effectors.Count; j++)
                {
                    if (isResume)
                    {
                        curPlayer.IkSystem.ResumeInteraction(effectors[j]);
                    }
                    else
                    {
                        curPlayer.IkSystem.StartInteraction(effectors[j], interaction.interactionObject, true);
                    }
                }
            }
        }

        #endregion

        #region 权限检测

        public AttachAttribute AttachAttribute
        {
            get { return LinkObject.GetAttribute() as AttachAttribute; }
        }

        /// <summary>
        /// 能否交互
        /// </summary>
        /// <returns></returns>
        public virtual bool CanDisplay()
        {
            bool canInteration = true;
            //过滤控制
            Player playerInstance = GlobalManager.Instance.Player;
            if (null == playerInstance)
            {
                return false;
            }

            var tmpState = GetSwitchState();
            //过滤状态
            switch (tmpState) //(AttachAttribute.mState)
            {
                case En_SwitchState.On:
                case En_SwitchState.Idle:
                    //todo:case other state by override 
                {
                    //状态是操作状态 ：On/Working/inactive ...
                    //说明要显示放下的按钮，这个时候需要判断交互对象是否是物体持有（角色）点击的操作
                    var player = LinkObject.GetParent();
                    canInteration = player == null || player.SceneInstance == null ||
                                    player == GlobalManager.Instance.Player;
                }
                    break;
                case En_SwitchState.Off:
                    //todo:case other state by override 
                {
                    //状态是操作状态 ：Off/Hide...
                    //说明要显示拿起的按钮，1、判断当前状态是否可交互，2、这个时候需要判断交互对象是否是物体持有（角色）
                    canInteration = Utilitys.OperatorStateCheck(playerInstance, EN_CharacterActionState.EN_CHA_Stay);

                    if (canInteration)
                    {
                        var player = LinkObject.GetParent() as Player;
                        if (player != null && !player.Equals(playerInstance))
                        {
                            canInteration = false;
                        }
                    }
                }
                    break;
            }
            if (!canInteration)
            {
                return false;
            }

            //过滤范围限定(取消判定)
            //if (isRange && !m_TriggerList.Contains((PlayerInstance)playerInstance.SceneInstance))
            //{
            //    canInteration = false;
            //}

            //过滤权限（操作技能）
            if (canInteration)
            {
                //需要权限判定
                canInteration = HasAuthority;
                if (!canInteration)
                {
                    Debug.Log("canInteration HasAuthority is false");
                }
            }

            return canInteration;
        }

        /// <summary>
        /// 能否拾取(过时)
        /// </summary>
        /// <returns></returns>
        public virtual bool InInterationArea()
        {
            //当前状态不为off的情况
            var player = GlobalManager.Instance.Player;
            if (player == null)
                return false;
            Vector3 direction = player.SceneInstance.GetDirection();
            Vector3 pos = player.SceneInstance.GetPosition();
            Vector3 targetPos = transform.position;
            //获取两者间距的平方（计算更快不用根号）
            float sqrDis = Vector3.SqrMagnitude(targetPos - pos);

            //检索区域参数【球面： vector（distance,angle）】- (5,150)
            //距离判定
            if (sqrDis <= Mathf.Pow(5, 2))
            {
                return true;
            }

            if (sqrDis <= Mathf.Pow(150, 2))
            {
                float num = Mathf.Acos(Vector3.Dot(pos.normalized, targetPos.normalized) * Mathf.Rad2Deg);
                if (num >= direction.y - 150 * 0.5 && num <= direction.y + 150 * 0.5)
                    return true;
            }
            return false;
        }

        #endregion

        #region 触发

        public TriggleType TriggleType
        {
            get { return TriggleType.CharactorDis_Triggle; }
        }

        public bool Check( BaseSceneInstance baseObj )
        {
            //Debug.Log("Check");
            PlayerInstance ssitNetworkPlayer = baseObj as PlayerInstance;
            if (!ssitNetworkPlayer || !ssitNetworkPlayer.HasAuthority || !Utilitys.CheckAuthiroty(this))
            {
                return false;
            }

            return true;
        }

        public void Enter( BaseSceneInstance baseObj )
        {
            //Debug.Log("Enter");
            PlayerInstance ssitNetworkPlayer = baseObj as PlayerInstance;
            m_TriggerList.Add(ssitNetworkPlayer);
            //AppFacade.Instance.SendNotification((ushort)ConstNotification.AppendVisualOperation, this);
        }

        public void Exit( BaseSceneInstance baseObj )
        {
            //Debug.Log("Exit");
            PlayerInstance ssitNetworkPlayer = baseObj as PlayerInstance;
            m_TriggerList.Remove(ssitNetworkPlayer);
            //AppFacade.Instance.SendNotification((ushort)ConstNotification.RemoveVisualOperation, this);
        }

        public BaseSceneInstance OnPostTriggle( Vector3 point )
        {
            return this;
        }

        public void Stay( BaseSceneInstance sceneObj )
        {
        }

        #endregion


        #region 回放

        public override void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            AttachAttribute attribute = savedState as AttachAttribute;
            if (attribute == null)
                return;

            //sync state
            SynchronizeState(attribute, isReset, isFristFrame);
            //sync other
            SynchronizeEx(attribute, isReset);
            //sync ik
            SynchronizeIk(attribute, isReset);
        }

        protected virtual void SynchronizeState( AttachAttribute savedState, bool isReset, bool isFristFrame )
        {
            if (AttachAttribute.mState == savedState.mState)
                return;

            AttachAttribute.mState = savedState.mState;
            switch (savedState.mState)
            {
                case En_SwitchState.On:
                {
                }
                    break;
                case En_SwitchState.Off:
                {
                }
                    break;
                case En_SwitchState.Working:
                {
                    //LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch, ((int)En_SwitchState.Working).ToString(),LinkObject.GetParent());
                }
                    break;
                case En_SwitchState.Hide:
                {
                    //LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch, ((int)En_SwitchState.Hide).ToString(),LinkObject.GetParent());
                }
                    break;
            }
        }

        protected virtual void SynchronizeIk( AttachAttribute savedState, bool isReset )
        {
            //sync ik
            if (AttachAttribute.Authority != savedState.Authority)
            {
                AttachAttribute.Authority = savedState.Authority;

                if (!string.IsNullOrEmpty(savedState.Authority))
                {
                    LinkObject.SetParent(savedState.Authority);

                    var player = ObjectManager.Instance.GetObject(savedState.Authority);
                    if (isIk)
                    {
                        if (isReset)
                        {
                            LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch,
                                ((int) En_SwitchState.On).ToString(), player);
                        }
                        else
                        {
                            OnPlayIK(LinkObject, EnPropertyId.Switch, ((int) En_SwitchState.On).ToString(), player);
                        }
                    }
                    else
                    {
                        LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch,
                            ((int) En_SwitchState.On).ToString(), null);
                    }
                }
                else
                {
                    var player = LinkObject.GetParent();
                    if (isIk)
                    {
                        if (isReset)
                        {
                            (player as Player)?.PlayerController.IkSystem.StopAll();
                            LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch,
                                ((int) En_SwitchState.Off).ToString(), player);
                        }
                        else
                        {
                            OnPlayIK(LinkObject, EnPropertyId.Switch, ((int) En_SwitchState.Off).ToString(), player);
                        }
                    }
                    else
                    {
                        //OnPlay(LinkObject, EnPropertyId.OnSwitch, ((int)savedState.mState).ToString());
                        LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch,
                            ((int) En_SwitchState.Off).ToString(), null);
                    }
                    LinkObject.SetParent((BaseObject) null);
                }
            }
        }

        protected virtual void SynchronizeEx( AttachAttribute savedState, bool isReset )
        {
        }

        #endregion
    }
}