using System;
using Framework.SceneObject;
using SsitEngine.Unity.Avatar;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    [Serializable]
    public class IkWeightParam
    {
        public AvatarIKGoal avatarIk;
        public Transform Trans;
    }

    /// <summary>
    /// 简单的UI交互行为
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerInstance))]
    public class ScriptIKWeight : MonoBase
    {
        public Transform BLPoint;
        public Transform BRPoint;
        private PlayerInstance m_instance;
        private Animator mAnimator;

        public void Start()
        {
            mAnimator = GetComponent<Animator>();
            m_instance = GetComponent<PlayerInstance>();
        }


        private void OnAnimatorIK( int layerIndex )
        {
            var player = m_instance.LinkObject;
            if (player == null || player.LoadStatu != EnLoadStatu.Inited)
                return;

            switch (player.State)
            {
                case EN_CharacterActionState.EN_CHA_MHQReady:
                {
                    HoldAnnihilator(player);
                }
                    break;
                case EN_CharacterActionState.EN_CHA_Strecher:
                {
                    OnStreach();
                }
                    break;
            }
            //if (mAnimator.GetCurrentAnimatorStateInfo(3).IsName("Stretcher"))
            //{
            //    mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            //    mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, BLPoint.position);

            //    mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            //    mAnimator.SetIKPosition(AvatarIKGoal.RightHand, BRPoint.position);
            //    //ani.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            //    //ani.SetIKRotation(AvatarIKGoal.RightHand, player.rotation);
            //}
        }


        #region Internal Members

        private void HoldAnnihilator( Player player )
        {
            if (mAnimator.GetCurrentAnimatorStateInfo(3).IsName("Outfire"))
            {
                var anni = player.GetInteractionObject(InvUseNodeType.InvAnnihilatorNode);
                if (anni != null && anni.SceneInstance is IIkWeight weight)
                {
                    var IkPos = weight.mWeightParam;
                    mAnimator.SetIKPositionWeight(IkPos.avatarIk, 1);
                    mAnimator.SetIKPosition(IkPos.avatarIk, IkPos.Trans.position);
                }
            }
            else
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            }
        }

        private void OnStreach()
        {
            if (mAnimator.GetCurrentAnimatorStateInfo(3).IsName("Stretcher"))
            {
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, BLPoint.position);

                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                mAnimator.SetIKPosition(AvatarIKGoal.RightHand, BRPoint.position);
            }
        }

        #endregion
    }
}