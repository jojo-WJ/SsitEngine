using Framework.Data;
using Framework.Logic;
using RootMotion.FinalIK;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    public class NpcInstance : BaseSceneInstance
    {
        private ThirdPersonController mController;

        public ThirdPersonController PlayerController
        {
            get
            {
                if (mController == null) mController = GetComponent<ThirdPersonController>();
                return mController;
            }

            set => mController = value;
        }

        public InteractionSystem IkSystem
        {
            get => null;

            set
            {
                //mInteractionSystem = value;
            }
        }


        public new Npc LinkObject => m_baseObject as Npc;

        #region Mono

        public virtual void Awake()
        {
            mController = GetComponent<ThirdPersonController>();
        }

        #endregion

        public override void OnSelect( bool state, object select )
        {
            base.OnSelect(state, select);

        }

        /// <summary>
        /// 属性改变回调
        /// </summary>
        /// <param name="propertyId">属性id</param>
        /// <param name="property">属性参数(字符串化)</param>
        /// <param name="data">自定义属性数据</param>
        public override void OnChangePropertyCallback( EnPropertyId propertyId, string property, object data = null )
        {
            switch (propertyId)
            {
                case EnPropertyId.Active:
                {
                    var active = property.ParseByDefault(true);
                    gameObject.SetActive(active);
                }
                    break;
                case EnPropertyId.Position:
                    OnMove(property.ParseByDefault(transform.position));
                    break;
                case EnPropertyId.Rotate:
                    OnRotate(property.ParseByDefault(transform.rotation));
                    break;
            }
        }

        #region 子类实现

        // Base
        public override bool HasAuthority
        {
            get
            {
                if (!GlobalManager.Instance.IsSync &&
                    LinkObject.GetAttribute().GroupId != ConstValue.c_sDefaultNpcGroupName) return true;
                return mController.syncCom.hasAuthority && mController.syncCom.isLocalPlayer;
            }
        }

        public override EnObjectType Type => EnObjectType.GamePlayer;

        public override void OnSelected( bool isIndicator = false )
        {
            // SsitDebug.Info("OnSelected");
        }

        public override void OnUnSelected( bool isIndicator = false )
        {
            // SsitDebug.Info("OnUnSelected");
        }

        public override void SetColliderActive( bool enable )
        {
            /*if (null == m_Colliders)
            {
                Debug.LogError("SetColliderActive error, ensure this base scene object had inited already please");
            }

            for (int i = 0; i < m_Colliders.Count; i++)
            {
                if (m_Colliders[i] == null)
                {
                    continue;
                }
                m_Colliders[i].enabled = enable;
            }

            if (m_Rigibody)
            {
                m_Rigibody.useGravity = enable;
                //m_Rigibody.isKinematic = !enable;
            }*/
        }

        #endregion

        #region Physic

        public void IngnoreColliders( Collider colliders )
        {
            Physics.IgnoreCollision(mController._capsuleCollider, colliders);
        }

        public void IngnoreColliders( Collider[] colliders )
        {
            for (var i = 0; i < colliders.Length; i++)
                if (colliders[i])
                    Physics.IgnoreCollision(mController._capsuleCollider, colliders[i]);
        }

        #endregion

        #region 回放

        public override void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            //base.SynchronizeProperties(savedState, isReset);


            var attribute = savedState as PlayerAttribute;
            if (attribute == null)
            {
                if (gameObject.activeSelf != savedState.isActive) gameObject.SetActive(savedState.isActive);
                return;
            }
            if (gameObject.activeSelf != savedState.isActive)
            {
                if (!gameObject.activeSelf)
                {
                    OnMove(attribute.position);
                    OnRotate(attribute.rotation);
                }
                gameObject.SetActive(savedState.isActive);
            }

            var curAttr = LinkObject.GetAttribute();

            if (isFristFrame)
                curAttr.ResetBaseTrack(attribute.mBaseTrack);

            SynchronizeMotor(attribute);

            SynchronizeInvEquip(attribute, curAttr);

            SynchronizeUseInvEquip(attribute, curAttr);

            SynchronizeState(attribute, curAttr);
        }

        public void SynchronizeMotor( PlayerAttribute attribute )
        {
            PlayerController.ApplyInputs(attribute);
            //OnMove(attribute.position);
            //OnRotate(attribute.rotation);
        }

        public void SynchronizeInputCommand( PlayerAttribute attribute )
        {
        }

        public void SynchronizeInvEquip( PlayerAttribute attribute, PlayerAttribute curAttr )
        {
            if (attribute.mEquips == null)
                return;

            //同步装备列表
            void InternalAddEquipCallback( BaseObject obj, object render, object data )
            {
                LinkObject.PutOnEquipment((Item) obj);
            }

            for (var i = 0; i < attribute.mEquips.Length; i++)
            {
                var equip = attribute.mEquips[i];
                var preEquip = curAttr.mEquips[i];

                curAttr.mEquips[i] = equip;
                //如果不相等
                if (preEquip != null && !preEquip.ExEquals(equip))
                {
                    var preItem = LinkObject.GetItemByInvSlot((InvSlot) i);

                    if (equip != null)
                        //换装
                        switch ((EnItemState) equip.state)
                        {
                            case EnItemState.IS_Normal:
                                SsitApplication.Instance.CreateItem(equip.guid, equip.dataId, InternalAddEquipCallback);
                                break;
                            case EnItemState.IS_Using:
                                LinkObject.UseEquip(preItem);
                                break;
                        }
                    else
                        //卸妆
                        LinkObject.PutOffEquipment(preItem);
                }
                else if (preEquip == null && equip != null)
                {
                    //换装
                    switch ((EnItemState) equip.state)
                    {
                        case EnItemState.IS_Normal:
                            SsitApplication.Instance.CreateItem(equip.guid, equip.dataId, InternalAddEquipCallback);
                            break;
                        case EnItemState.IS_Using:
                            SsitDebug.Error("SynchronizeInvEquip异常");
                            break;
                    }
                }
            }
        }

        public void SynchronizeUseInvEquip( PlayerAttribute attribute, PlayerAttribute curAttr )
        {
            for (var i = 0; i < attribute.mUseItem.Length; i++)
            {
                var equip = attribute.mUseItem[i];
                var preEquip = curAttr.mUseItem[i];

                curAttr.mUseItem[i] = equip;
                //如果不相等
                if (equip != preEquip)
                {
                    if (!string.IsNullOrEmpty(equip))
                    {
                        var temp = ObjectManager.Instance.GetObject(equip);
                        LinkObject.AddInteractionObject((InvUseNodeType) i, temp);
                    }
                    else
                    {
                        LinkObject.RemoveInteractionObject((InvUseNodeType) i, null);
                    }
                }
            }
        }

        public void SynchronizeState( PlayerAttribute attribute, PlayerAttribute curAttr )
        {
            var state = attribute.mState;
            var preState = curAttr.mState;
            curAttr.mState = state;
            if (state != preState) LinkObject.ChangeState(state);
        }

        #endregion
    }
}