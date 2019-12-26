using Framework;
using Framework.SceneObject;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace SsitEngine.Unity.SceneObject
{
    /// <summary>
    /// 气体检测状态
    /// </summary>
    public class CharacterStateDetectorGas : CharacterState
    {
        private TimerEventTask mOnAnimationEndFunc;
        private TimerEventTask mOnAnimationFunc;

        public CharacterStateDetectorGas( Character player, string animName = "DetectGas", int animLayer = 3 )
            : base(player, EN_CharacterActionState.EN_CHA_DetectorGas, animName, animLayer)
        {
            mOnAnimationFunc = null;
            mOnAnimationEndFunc = null;
        }


        protected override void OnEnter( IFsm<Character> fsm )
        {
            if (mCharacter == null) return;
            mCharacter.PlayAnimation(mAnimation, mAnimLayer, true);

            mOnAnimationFunc =
                SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, 0.6f, 0, OnAnimationFunc);
            mOnAnimationEndFunc =
                SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, 2.5f, 0, OnAnimationEndFunc);
        }


        private void OnAnimationFunc( TimerEventTask eve, float timeelapsed, object data )
        {
            var item = mCharacter.GetItemByInvSlot(InvSlot.InvChestBagLeft);
            if (item != null) mCharacter.UseEquip(item);

            mOnAnimationFunc =
                SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, 1.4f, 0, OnPopTipFunc);
        }


        private void OnAnimationEndFunc( TimerEventTask eve, float timeelapsed, object data )
        {
            var item = mCharacter.GetItemByInvSlot(InvSlot.InvChestBagLeft);
            if (item != null) mCharacter.UnUseEquip(item);

            mOnAnimationEndFunc = null;

            mCharacter.ChangeProperty(this, EnPropertyId.State, ((int) EN_CharacterActionState.EN_CHA_Stay).ToString());
        }

        private void OnPopTipFunc( TimerEventTask eve, float timeelapsed, object data )
        {
            //检测自身范围内有无气体
            var objects = ObjectManager.Instance.GetSceneObject(GlobalManager.Instance.CachePlayer,
                EnFactoryType.SceneObjectFactory,
                ObjectManager.SearchRelation.CR_Gas, ObjectManager.SearchAreaType.SA_Fan, new Vector2(80, 360));

            mOnAnimationFunc = null;
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (isShutdown)
                return;

            if (mOnAnimationFunc != null)
                SsitApplication.Instance.RemoveTimerEvent(mOnAnimationFunc);
            if (mOnAnimationEndFunc != null)
                SsitApplication.Instance.RemoveTimerEvent(mOnAnimationEndFunc);
            if (mCharacter == null)
                return;
        }
    }
}