using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateAssign : CharacterState
    {
        public CharacterStateAssign( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_Assign, animName, animLayer)
        {
            mAnimation = "Assign";
            mAnimLayer = 2;
        }

        protected override void OnEnter( IFsm<Character> fsm )
        {
            mCharacter.PlayAnimation(mAnimation, mAnimLayer);

            var patientObj = mCharacter?.GetInteractionObject(InvUseNodeType.InvAssignNode);

            if (patientObj != null)
            {
                var root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvAssignNode);
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
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;

            mCharacter.PlayAnimation("Null", mAnimLayer);
            //mCharacter.thirdPersonManipulator.ChangeSpeed();
            var root = mCharacter.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvAssignNode);
            if (!root)
            {
                SsitDebug.Error("角色挂点异常");
                return;
            }

            root.SetActive(false);
            //隐藏病人
            //Transform assignPatient = mCharacter.transform.Find("AssignHost");
            //if (assignPatient)
            //{
            //    assignPatient.gameObject.SetActive(false);
            //}
            var patientObj = mCharacter?.GetInteractionObject(InvUseNodeType.InvAssignNode);

            if (patientObj != null && patientObj.SceneInstance)
            {
                patientObj.SceneInstance.transform.position = root.transform.position;
                patientObj.SetVisible(true);
            }
        }
    }
}