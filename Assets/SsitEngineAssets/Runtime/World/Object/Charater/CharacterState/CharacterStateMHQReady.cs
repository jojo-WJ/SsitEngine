using Framework;
using Framework.SceneObject;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateMHQReady : CharacterState
    {
        public CharacterStateMHQReady( Character player, string animName = "Outfire", int animLayer = 3 )
            : base(player, EN_CharacterActionState.EN_CHA_MHQReady, animName, animLayer)
        {
        }

        protected override void OnInit( IFsm<Character> fsm )
        {
            //mPreviouState = fsm.GetState( (int)EN_CharacterActionState.EN_CHA_MHQAttach );
        }

        protected override void OnEnter( IFsm<Character> fsm )
        {
            mCharacter?.PlayAnimation("Outfire", 3);
            var anni = mCharacter?.GetInteractionObject(InvUseNodeType.InvAnnihilatorNode);
            if (anni != null)
                anni.OnChangeProperty(anni, EnPropertyId.OnSwitch, ((int) En_SwitchState.Show).ToString());
        }


        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            mCharacter?.PlayAnimation("Null", 3);

            var anni = mCharacter?.GetInteractionObject(InvUseNodeType.InvAnnihilatorNode);
            if (anni != null)
                anni.OnChangeProperty(anni, EnPropertyId.OnSwitch, ((int) En_SwitchState.Idle).ToString());
        }

        protected override void OnDestroy( IFsm<Character> fsm )
        {
            var anni = mCharacter?.GetInteractionObject(InvUseNodeType.InvAnnihilatorNode);
            if (anni != null)
                anni.OnChangeProperty(anni, EnPropertyId.OnSwitch, ((int) En_SwitchState.Off).ToString(), mCharacter);
            base.OnDestroy(fsm);
        }
    }
}