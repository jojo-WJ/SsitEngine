/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述： 技能实体类                                                   
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/07/30                        
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using Framework.Data;
using Framework.Helper;
using Framework.SsitInput;
using Framework.Utility;
using SSIT.Events;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.UI;
using Table;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.SceneObject
{
    public enum EnSkillState
    {
        IS_Enable = 0,
        IS_Disable,
        IS_Excuting
    }

    //    class UnityEvent<object>:UnityEngine.Events.UnityEvent<object>
    //    {
    //        
    //    } 
    public class Skill : BaseObject
    {
        protected BoolEvent mEvent = new BoolEvent();


        /// <summary>
        /// 我的归属
        /// </summary>
        protected BaseObject mOwner;

        /// <summary>
        /// 我的属性信息
        /// </summary>
        private SkillAttribute m_mSkillAttribute;


        protected EnSkillState mSkillState;

        public Skill( string mGuid ) : base(mGuid)
        {
            mOwner = null;
        }

        public EnSkillState SkillState => mSkillState;

        public void SetAttribute( SkillAttribute skillAttribute )
        {
            m_mSkillAttribute = skillAttribute;
            m_mSkillAttribute.SetOwner(this);
            mSkillState = (EnSkillState) m_mSkillAttribute.DefaultState;
        }

        private void ParseEvent( bool isOn )
        {
            if (SkillState == EnSkillState.IS_Disable) return;
            m_mSkillAttribute.IsOn = isOn;
            switch (m_mSkillAttribute.EventType)
            {
                case En_SkillEventType.OpenForm:
                    var uiform = ParseSkillParaToUIForm(m_mSkillAttribute.EventParam);
                    Facade.Instance.SendNotification((ushort) UIMsg.OpenForm,
                        new UIParam
                        {
                            formId = (int) uiform,
                            isAsync = true,
                            OnCloseCallBack = x =>
                            {
                                //Facade.Instance.SendNotification((ushort)UICharacterSkillPanelEvent.SetSkillState, mSkillInfo.skillId, false);
                            }
                        }, m_mSkillAttribute.skillId, m_mSkillAttribute.ExtraMsgParm);
                    SetState(EnSkillState.IS_Excuting);
                    break;
                case En_SkillEventType.CreateObject:
                    //mEvent.AddListener(() =>
                    //{
                    //    //SsitApplication.Instance.SpawnObject(null,mSkillInfo.EventParam);
                    //});
                    Facade.Instance.SendNotification((ushort) En_SkillEventType.CreateObject,
                        m_mSkillAttribute.EventParam);
                    break;
                case En_SkillEventType.PlayMHQAniml: //举起灭火器动画
                    if (isOn)
                    {
                        var mhq = (mOwner as Player).GetInteractionObject(InvUseNodeType.InvAnnihilatorNode);
                        (mOwner as Player).ChangeProperty(this, EnPropertyId.State,
                            ((int) EN_CharacterActionState.EN_CHA_MHQReady).ToString());
                        //Facade.Instance.SendNotification((ushort)UIOperationsFormEvent.OpenOperationForm, mhq);
                    }
                    else
                    {
                        (mOwner as Player).ChangeProperty(this, EnPropertyId.State,
                            ((int) EN_CharacterActionState.EN_CHA_MHQAttach).ToString());
                        //Facade.Instance.SendNotification((ushort)UIOperationsFormEvent.OpenOperationForm, mOwner);
                    }
                    break;
                //case En_SkillEventType.UseWaterPipe://举起灭火器动画
                //    {
                //        BaseObject mhq = (mOwner as Player).GetInteractionObject(InvUseNodeType.InvWaterPipeNode);
                //        Facade.Instance.SendNotification((ushort)UIOperationsFormEvent.OpenOperationForm, mhq);
                //    }
                //    break;
                case En_SkillEventType.AimToFire: //使用灭火器灭火
                    //case En_SkillEventType.PipeAimToFire://使用灭火器灭火
                    if (isOn)
                    {
                        SetState(EnSkillState.IS_Excuting);
                        mOwner.OnChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Begin).ToString(),
                            mOwner.GetParent());
                        mOwner.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Working).ToString(),
                            mOwner.GetParent());
                    }
                    else
                    {
                        SetState(EnSkillState.IS_Enable);
                        mOwner.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Hide).ToString(),
                            mOwner.GetParent());
                    }
                    break;
                case En_SkillEventType.OnUseGasDetect:

                    //经项目经理确定界面不需要，仅需要弹窗后显示汇报正常就可以了
                    //if (isOn)
                {
                    mOwner.ChangeProperty(this, EnPropertyId.State,
                        ((int) EN_CharacterActionState.EN_CHA_DetectorGas).ToString());
                    //Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.GasInfoForm, mSkillInfo.skillId);
                }
                    //else
                    //{
                    //    mOwner.ChangeProperty(this, EnPropertyId.State, ((int)EN_CharacterActionState.EN_CHA_Stay).ToString());
                    //    Facade.Instance.SendNotification((ushort)UIMsg.CloseForm, En_UIForm.GasInfoForm, mSkillInfo.skillId);
                    //}
                    break;
                case En_SkillEventType.ActiveXFP: //打开/关闭消防炮
                    if (isOn)
                        mOwner.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Working).ToString(),
                            mOwner.GetParent());
                    else
                        mOwner.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Hide).ToString(),
                            mOwner.GetParent());
                    break;
                case En_SkillEventType.OpenRemoteSwitch: //打开远程开关
                    //Facade.Instance.SendNotification((ushort)UIOperationsFormEvent.OpenRemoteSwitch, mOwner);
                    break;
                case En_SkillEventType.OnSwitch:
                    mOwner.OnChangeProperty(this, EnPropertyId.OnSwitch, GetAttribute().EventParam);
                    break;
                case En_SkillEventType.OnCharacterState:
                {
                    if (isOn)
                        mOwner.ChangeProperty(this, EnPropertyId.State, GetAttribute().EventParam);
                    //Facade.Instance.SendNotification((ushort) UIOperationsFormEvent.CloseOperationForm);
                    else
                        mOwner.ChangeProperty(this, EnPropertyId.State, GetAttribute().ExtraMsgParm);
                }
                    break;
                case En_SkillEventType.StartDrawSEntry:
                {
                    if (isOn)
                    {
                        var canInteration =
                            Utilitys.OperatorStateCheck(mOwner as Player, EN_CharacterActionState.EN_CHA_Stay);
                        if (canInteration)
                        {
                            (mOwner as Player)?.ChangeProperty(this, EnPropertyId.State,
                                ((int) EN_CharacterActionState.EN_CHA_Sentry).ToString());
                        }
                    }
                    else
                    {
                        if (((Player) mOwner).State == EN_CharacterActionState.EN_CHA_Sentry)
                        {
                            ((Player) mOwner)?.ChangeProperty(this, EnPropertyId.State,
                                ((int) EN_CharacterActionState.EN_CHA_Stay).ToString());
                        }
                    }
                }
                    break;
                case En_SkillEventType.UseWaterPipe:
                {
                    var o = (mOwner as Player)?.GetInteractionObject(InvUseNodeType.InvWaterPipeNode);
                    if (isOn)
                        o.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Working).ToString(),
                            mOwner);
                    else
                        o.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Hide).ToString(), mOwner);
                }
                    break;
                default:
                    Facade.Instance.SendNotification((ushort) m_mSkillAttribute.EventType,
                        m_mSkillAttribute.EventParam);
                    break;
            }
        }

        public bool ExcuteSkill( bool isOn )
        {
            if (mEvent == null) return false;
            mEvent.Invoke(isOn);
            SetState(EnSkillState.IS_Excuting);
            InputManager.Instance.ResetFocus();
            return true;
        }

        /// <summary>
        /// 设置技能状态
        /// </summary>
        /// <param name="state"></param>
        public void SetState( EnSkillState state )
        {
            mSkillState = state;
        }

        public new SkillAttribute GetAttribute()
        {
            return m_mSkillAttribute;
        }

        public SkillAttribute GetSkillInfo()
        {
            return m_mSkillAttribute;
        }

        public void SetOwner( BaseObject p )
        {
            mOwner = p;
        }

        public BaseObject GetOwner()
        {
            return this;
        }

        /// <summary>
        /// 刷新技能栏状态
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="old"></param>
        /// <param name="current"></param>
        internal void RefreshSkillByUseNode( InvUseNodeType nodeType, BaseObject old, BaseObject current )
        {
            if (mOwner is Player player)
            {
                if ((int) nodeType != m_mSkillAttribute.SkillConditionParam)
                    return;

                var hasEquip = player.HasInteractionObject(nodeType);
                SetState(hasEquip ? EnSkillState.IS_Enable : EnSkillState.IS_Disable);
                if (!hasEquip) m_mSkillAttribute.IsOn = false; //无论是否可用，此时技能都处于关闭状态
                //Facade.Instance.SendNotification((ushort)UICharacterSkillPanelEvent.RefreshSkillState, this, hasEquip);
            }
        }

        /// <summary>
        /// 将技能参数转化为UIForm
        /// </summary>
        /// <param name="skillPara"></param>
        /// <returns></returns>
        private En_UIForm ParseSkillParaToUIForm( string skillPara )
        {
            var uiform = En_UIForm.Canvas;
            if (int.TryParse(skillPara, out var result))
            {
                uiform = (En_UIForm) result;
            }
            else if (Enum.TryParse(skillPara, out uiform))
            {
            }
            return uiform;
        }


        /// <summary>
        /// 销毁
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            mData = null;
            Unload();
        }


        #region 实体表现初始化

        protected override void Create( GameObject warpObject = null )
        {
            //SsitDebug.Debug(mRepresent + "Create" + warpObject);
            if (mRepresent != null)
            {
                //todo:统一处理销毁的问题（是用池管理，还是其他缓存方式）
                Object.Destroy(mRepresent);
                mRepresent = null;
            }

            if (m_loadStatu == EnLoadStatu.Loading)
            {
                if (warpObject == null)
                {
                    // //todo：资源管理器加载资源 异步话加入资源队列 回调执行后续，同步话继续
                    if (!string.IsNullOrEmpty(mResouceName))
                    {
                        //todo:load skill xml


                        //todo:parse skill config xml
                    }
                    else
                    {
                        OnCreateComplete(warpObject);
                    }
                }
                else
                {
                    OnCreateComplete(warpObject);
                }
            }
        }

        protected override void OnCreateComplete( GameObject represent )
        {
            //            if (represent != null)
            //            {
            mRepresent = represent;

            m_loadStatu = EnLoadStatu.Loaded;

            OnInit();

            //instance.guid = mGuid;
            // Add event
            //ParseEvent();
            mEvent.AddListener(ParseEvent);

            ParseSkillCondition();

            if (mOnCreated != null)
                mOnCreated(this, mRepresent, mData);


            //}
            m_loadStatu = EnLoadStatu.Inited;
        }

        private void ParseSkillCondition()
        {
            var skillConditionType = (En_SkillConditionType) m_mSkillAttribute.SkillConditionType;
            switch (skillConditionType)
            {
                case En_SkillConditionType.EN_Equip:
                    //skillType = int.Parse(mSkillInfo.SkillConditionParam);
                    //skillState = (mOwner.GetAttribute() as PlayerAttribute).HasSlot(skillType);
                    break;
                case En_SkillConditionType.EN_UseEquip:
                    //skillType = int.Parse(mSkillInfo.SkillConditionParam);
                {
                    if (mOwner is Player player)
                        //player.OnUseChanged -= RefreshSkillByUseNode;
                        player.OnUseChanged += RefreshSkillByUseNode;
                }
                    break;
                case En_SkillConditionType.EN_UseSkillState:
                    //skillType = int.Parse(mSkillInfo.SkillConditionParam);
                {
                    if (mOwner is Player player)
                        //player.OnUseChanged -= RefreshSkillByUseNode;
                        player.OnUseChanged += RefreshSkillByUseNode;
                }
                    break;
            }
        }

        #endregion
    }
}