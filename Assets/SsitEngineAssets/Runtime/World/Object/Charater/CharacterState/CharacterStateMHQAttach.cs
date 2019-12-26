using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateMHQAttach : CharacterState
    {
        public CharacterStateMHQAttach( Character player, string animName = "HoldFireExp", int animLayer = 1 )
            : base(player, EN_CharacterActionState.EN_CHA_MHQAttach, animName, animLayer)
        {
        }

        protected override void OnEnter( IFsm<Character> fsm )
        {
            mCharacter.PlayAnimation(mAnimation, mAnimLayer);
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            mCharacter.PlayAnimation("Null", mAnimLayer);
        }
    }
}