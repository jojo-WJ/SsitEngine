using Framework.Helper;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;
using UnityEngine;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateStrecher : CharacterState
    {
        public CharacterStateStrecher( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_Strecher)
        {
            mAnimation = "Stretcher";
            mAnimLayer = 3;
        }

        /// <summary>
        /// 有限状态机状态初始化时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected override void OnInit( IFsm<Character> fsm )
        {
        }

        /// <summary>
        /// 有限状态机状态进入时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected override void OnEnter( IFsm<Character> fsm )
        {
            //if (mCharacter.OnHUDChange != null)
            //{
            //    mCharacter.OnHUDStateChange.Invoke("救助中......");
            //}

            mCharacter.PlayAnimation(mAnimation, mAnimLayer);

            // to sync assign's equips
            if (mCharacter?.GetInteractionObject(InvUseNodeType.InvStrechNode) is Player assign)
            {
                //anni.OnChangeProperty(anni, EnPropertyId.OnSwitch, ((int)En_SwitchState.Show).ToString());

                var root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvStrechNode);
                if (!root)
                {
                    SsitDebug.Error("角色挂点异常");
                    return;
                }
                var equipAttach = root.GetComponentInChildren<EquipController>(true);
                equipAttach.InitAttachPoint();
                //Debug.LogError("OnEnter" + assign.GetAttribute().Name);
                var equips = assign.GetEquipItem();
                if (equips != null)
                    equipAttach.Clone(equips);
                var cc = root.GetComponentInChildren<vAssignController>(true);
                // ReSharper disable once Unity.NoNullPropagation
                cc?.InitHUD(ObjectHelper.GetPlayerName(assign), string.Empty);
                root.SetActive(true);

                assign.SetVisible(false);
            }

            //to sync patient's equips
            var patientObj = mCharacter?.GetInteractionObject(InvUseNodeType.InvStrechPatientNode);

            if (patientObj != null)
            {
                var root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvStrechPatientNode);
                if (!root)
                {
                    SsitDebug.Error("角色挂点异常");
                    return;
                }
                var equipAttach = root.GetComponentInChildren<EquipController>(true);
                if (equipAttach != null)
                {
                    equipAttach.InitAttachPoint();

                    var patient = patientObj.SceneInstance as NpcInstance;
                    if (equipAttach != null && patient != null)
                        equipAttach.Clone(patient.LinkObject.GetEquipItem());
                    else
                        SsitDebug.Error("角色引用异常");
                }
                root.SetActive(true);

                patientObj.SetVisible(false);
            }

            //vAssignController vc = simulatePatient.GetComponentInChildren<vAssignController>();
            //if (vc)
            //{
            //    vc.ChangeSpeed();
            //}
            //mCharacter.thirdPersonManipulator.ChangeSpeed();
        }


        /// <summary>
        /// 有限状态机状态离开时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;
            mCharacter.PlayAnimation("Null", mAnimLayer);
            //mCharacter.thirdPersonManipulator.ChangeSpeed();

            //隐藏担架及病人
            var root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvStrechPatientNode);
            if (!root)
            {
                SsitDebug.Error("角色挂点异常");
                return;
            }
            root.SetActive(false);
            //显示病人
            var patientObj = mCharacter?.GetInteractionObject(InvUseNodeType.InvStrechPatientNode);

            if (patientObj != null && patientObj.SceneInstance)
            {
                var temp = root.transform.position;
                patientObj.SceneInstance.transform.position =
                    new Vector3(temp.x, mCharacter.SceneInstance.transform.position.y, temp.z);
                patientObj.SetVisible(true);
            }

            //隐藏协作人员
            root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvStrechNode);
            if (!root)
            {
                SsitDebug.Error("角色挂点异常");
                return;
            }
            root.SetActive(false);

            //显示协作人员
            patientObj = mCharacter?.GetInteractionObject(InvUseNodeType.InvStrechNode);

            if (patientObj != null)
            {
                patientObj.SceneInstance.transform.position = root.transform.position;
                patientObj.SetVisible(true);
            }
        }
    }
}