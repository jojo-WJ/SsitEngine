using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateSentry : CharacterState
    {
        public CharacterStateSentry( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_Sentry, animName, animLayer)
        {
        }

        //protected override bool CouldEnter(IFsm<FrameworkNetworkPlayer> fsm)
        //{
        //    return fsm.CurrentState.Id == (int)EN_CharacterActionState.EN_CHA_Stay;

        //}

        protected override void OnEnter( IFsm<Character> fsm )
        {
            mCharacter.PlayAnimation("Traffic", 5);
            mCharacter.PlayerController.lockInput = true;
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            mCharacter.PlayAnimation("Null", 5);
            mCharacter.PlayerController.lockInput = false;
        }
    }
}