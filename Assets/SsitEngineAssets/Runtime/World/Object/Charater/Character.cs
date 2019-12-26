/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/30 11:44:31                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.HUD;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    public class Character : BaseObject
    {
        public delegate void OnItemChanged( Item old, Item current );

        public delegate void UseChangedHandler( InvUseNodeType nodeType, BaseObject old, BaseObject current );

        protected EquipController m_equipController;

        protected ThirdPersonController m_playerController;

        protected Item[] mEquipmentList; //装备列表

        //hud操作
        protected HudElement mHud;
        protected BaseObject[] mUsingItems; //物体交互列表
        public UseChangedHandler OnUseChanged;

        public Character( string mGuid ) : base(mGuid)
        {
        }


        public HudElement Hud
        {
            get => mHud;
            set => mHud = value;
        }

        public ThirdPersonController PlayerController => m_playerController;

        public EquipController EquipController => m_equipController;

        public new CharacterAttribute GetAttribute()
        {
            return mAttribute as CharacterAttribute;
        }

        #region Property

        public override void ChangeProperty( object sender, EnPropertyId propertyId, string param, object data = null )
        {
            base.ChangeProperty(sender, propertyId, param, data);
        }

        public override void OnChangeProperty( object sender, EnPropertyId propertyId, string param,
            object data = null )
        {
            base.OnChangeProperty(sender, propertyId, param, data);
        }

        #endregion

        #region HUD

        public virtual void InitHUD( Transform trans, string showName, string message )
        {
            if (mHud == null) mHud = trans.Find("HeadHost")?.GetComponent<HudElement>();

            if (mHud != null)
            {
                mHud.AttachTo(NavigationElementType.HUD, true);
                ChangeHUDText(showName, message);
            }
        }

        public void RemoveHUD()
        {
            if (mHud != null) mHud.AttachTo(NavigationElementType.HUD, false);
        }

        public void ChangeHUDText( string showName, string message )
        {
            if (mHud != null)
                if (mHud.Hud != null)
                {
                    mHud.Hud.ChangeNameText(showName);
                    mHud.Hud.ChangeStateText(message);
                }
        }

        #endregion

        #region FSM

        protected Fsm<Character> mFsm;

        public EN_CharacterActionState State => (EN_CharacterActionState) mFsm.CurrentState.GetStateId();

        protected virtual void InitFsm()
        {
            mFsm = FsmHelper.CreateFsm(Guid, this);
            //access: 等待状态接入
            mFsm.OnStateCheckHandler = OnStateCheckCallback;
            mFsm.Start((int) EN_CharacterActionState.EN_CHA_Stay);
        }

        public void AddStateChagneCallBack( FsmStateChangeHandler<Character> func )
        {
            mFsm.OnStateChangedHandler += func;
        }

        public bool ChangeState( EN_CharacterActionState state )
        {
            if (mFsm == null) return false;

            if (mFsm.ChangeState((int) state))
            {
                SetProperty(EnPropertyId.OnState, ((int) state).ToString());
                return true;
            }
            return false;
        }

        public void SubStateEvent( EN_CharacterActionState state, int eventid, FsmEventHandler<Character> func )
        {
            if (mFsm == null) return;
            mFsm.GetState((int) state).SubscribeEvent(eventid, func);
        }

        public void UnSubStateEvent( EN_CharacterActionState state, int eventid, FsmEventHandler<Character> func )
        {
            if (mFsm == null) return;
            mFsm.GetState((int) state).UnsubscribeEvent(eventid, func);
        }

        protected void UpdateFsm( float elapsed )
        {
            if (mFsm != null) mFsm.OnUpdate(elapsed);
        }

        protected void DestoryFsm()
        {
            mFsm?.Shutdown();
        }

        protected bool OnStateCheckCallback( object sender, int curstate, int tarstate )
        {
            if (curstate == tarstate) return false;

            switch ((EN_CharacterActionState) tarstate)
            {
                case EN_CharacterActionState.EN_CHA_MHQAttach:
                    if ((EN_CharacterActionState) tarstate == EN_CharacterActionState.EN_CHA_Interactoin) return true;
                    break;
                case EN_CharacterActionState.EN_CHA_Interactoin:
                    if ((EN_CharacterActionState) tarstate == EN_CharacterActionState.EN_CHA_Stay) return true;

                    break;
            }
            return true;
        }

        #endregion

        #region 物体交互

        /// <summary>
        /// 有该种类型装备
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public bool HasInteractionObject( InvUseNodeType nodeType )
        {
            if (mUsingItems == null || mUsingItems.Length <= (int) nodeType) return false;

            return mUsingItems[(int) nodeType] != null;
        }

        public BaseObject GetInteractionObject( InvUseNodeType node )
        {
            if (mUsingItems == null)
                return null;
            var index = (int) node;
            return mUsingItems[index];
        }


        /// <summary>
        /// 对接交互对象
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="useSlot"></param>
        /// <param name="useNodeTypeType"></param>
        /// <param name="handler"></param>
        /// <param name="transationState"></param>
        public void AttachInteractionObject( BaseObject sceneObject, InvUseSlot useSlot, InvUseNodeType useNodeTypeType,
            GameObject handler, EN_CharacterActionState transationState )
        {
            AddInteractionObject(useNodeTypeType, sceneObject);

            m_equipController?.Attach(useSlot, useNodeTypeType, handler);

            if (transationState != EN_CharacterActionState.EN_CHA_None) ChangeState(transationState);
        }

        /// <summary>
        /// 分离交互对象
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="useSlot"></param>
        /// <param name="useNodeTypeType"></param>
        /// <param name="handler"></param>
        public void DetachInteractionObject( BaseObject sceneObject, InvUseSlot useSlot, InvUseNodeType useNodeTypeType,
            GameObject handler )
        {
            RemoveInteractionObject(useNodeTypeType, sceneObject);
            m_equipController?.Dettach(useSlot, useNodeTypeType, handler);
            ChangeState(EN_CharacterActionState.EN_CHA_Stay);
        }


        //TODO  跟随交互

        /// <summary>
        /// 对接交互对象
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="useSlot"></param>
        /// <param name="useNodeTypeType"></param>
        /// <param name="handler"></param>
        /// <param name="transationState"></param>
        public void FollowInteractionObject( BaseObject sceneObject, InvUseSlot useSlot, InvUseNodeType useNodeTypeType,
            GameObject handler, EN_CharacterActionState transationState )
        {
            AddInteractionObject(useNodeTypeType, sceneObject);

            //m_equipController?.Follow(useSlot, useNodeTypeType, handler);

            if (transationState != EN_CharacterActionState.EN_CHA_None) ChangeState(transationState);
        }

        /// <summary>
        /// 分离交互对象
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="useSlot"></param>
        /// <param name="useNodeTypeType"></param>
        /// <param name="handler"></param>
        public void DeFollowInteractionObject( BaseObject sceneObject, InvUseSlot useSlot,
            InvUseNodeType useNodeTypeType, GameObject handler )
        {
            RemoveInteractionObject(useNodeTypeType, sceneObject);
            //m_equipController?.DeFollow(useSlot, useNodeTypeType, handler);
            ChangeState(EN_CharacterActionState.EN_CHA_Stay);
        }

        public void AddInteractionObject( InvUseNodeType useNodeTypeType, BaseObject sceneObject )
        {
            if (mUsingItems == null) mUsingItems = new BaseObject[(int) InvUseNodeType.MaxValue];

            if (sceneObject == null) return;

            var temp = mUsingItems[(int) useNodeTypeType];
            mUsingItems[(int) useNodeTypeType] = sceneObject;
            GetAttribute()?.NotificationUseChange((int) useNodeTypeType, sceneObject.Guid);
            OnUseChanged?.Invoke(useNodeTypeType, temp, sceneObject);
        }

        public void RemoveInteractionObject( InvUseNodeType useNodeTypeType, BaseObject sceneObject )
        {
            if (mUsingItems == null)
                return;
            var temp = mUsingItems[(int) useNodeTypeType];
            mUsingItems[(int) useNodeTypeType] = null;
            GetAttribute()?.NotificationUseChange((int) useNodeTypeType, null);
            OnUseChanged?.Invoke(useNodeTypeType, temp, null);
        }

        #endregion

        #region 装备

        public Item[] GetEquipItem()
        {
            return mEquipmentList;
        }

        public bool HasEquip( int equipId )
        {
            if (mEquipmentList == null || mEquipmentList.Length == 0) return false;

            foreach (var item in mEquipmentList)
                if (item != null && item.GetAttribute().Id == equipId)
                    return true;
            return false;
        }


        /// <summary>
        /// 根据装备巢，获取该装备巢上的装备
        /// </summary>
        /// <param name="slot">装备巢类型</param>
        /// <returns></returns>
        public Item GetItemByInvSlot( InvSlot slot )
        {
            if (slot == InvSlot.InvSlotNone) return null;

            return mEquipmentList[(int) slot];
        }

        public BaseObject[] GetUserItem()
        {
            return mUsingItems;
        }

        /// <summary>
        /// 穿上装备
        /// </summary>
        /// <param name="item"></param>
        /// <param name="func"></param>
        public void PutOnEquipment( Item item, OnItemChanged func = null )
        {
            if (m_equipController == null)
                return;

            if (mEquipmentList == null)
            {
                var count = (int) InvSlot.MaxValue;
                mEquipmentList = new Item[count];
            }

            var attr = item.GetAttribute();
            if (attr.Slot == InvSlot.InvSlotNone) return;

            var pre = mEquipmentList[(int) attr.Slot];

            attr.SetParent(this);
            item.SetState(EnItemState.IS_Normal);
            mEquipmentList[(int) attr.Slot] = item;

            if (func != null)
                func(pre, item);

            m_equipController.Equip(attr.Slot, pre, item, GetEquipOffsetBySlot((int) attr.Slot));

            GetAttribute()?.NotificationEquipChange((int) attr.Slot, item);
        }

        /// <summary>
        /// 卸下装备(暂时用不到)
        /// </summary>
        /// <param name="item"></param>
        public void PutOffEquipment( Item item, int equipIndex = -1 )
        {
            if (item == null || m_equipController == null) return;
            var attr = item.GetAttribute();

            //根据此装备的装备巢尝试获取该装备巢上的初始装备 
            //如果获取成功，就将初始装备穿上  获取失败就删除该装备
            //[备注]：回放模式下不需要自动检测（一切按照回放数据进行还原）
            //if (GlobalManager.Instance.ReplayMode != ActionMode.PLAY)
            {
                var orignItem = GetAttribute().GetOrignEquipBySlot((int) attr.Slot);
                if (orignItem != null)
                {
                    PutOnEquipment(orignItem);
                    return;
                }
            }

            m_equipController.DestroyEquip(item);
            mEquipmentList[(int) attr.Slot] = null;
            GetAttribute()?.NotificationEquipChange((int) attr.Slot, null);
        }

        public void UseEquip( Item item )
        {
            var attr = item.GetAttribute();
            if (attr.Slot == InvSlot.InvSlotNone)
                return;
            item.Use();
            m_equipController.UseEquip(item);
            GetAttribute()?.NotificationEquipChange((int) attr.Slot, item);
        }

        public void UnUseEquip( Item item )
        {
            var attr = item.GetAttribute();
            if (attr.Slot == InvSlot.InvSlotNone)
                return;
            item.UnUse();
            m_equipController.UnUseEquip(item);
            GetAttribute()?.NotificationEquipChange((int) attr.Slot, item);
        }

        protected void NotificationItemStateChange( InvSlot attrSlot, Item item )
        {
            item.Use();
        }

        private Vector3 GetEquipOffsetBySlot( int slot )
        {
            /*var playAttr = GetAttribute();
            if (playAttr.mEquipOffsetMap.ContainsKey(slot))
            {
                return playAttr.mEquipOffsetMap[slot];
            }*/
            return Vector3.zero;
        }

        protected void DestoryAllEquip()
        {
            //todo:
        }

        #endregion

        #region 消息处理

        #endregion
    }
}