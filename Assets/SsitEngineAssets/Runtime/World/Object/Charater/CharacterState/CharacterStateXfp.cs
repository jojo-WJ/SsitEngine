using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateXfp : CharacterState
    {
        public CharacterStateXfp( Character player, string animName = "SideWay", int animLayer = 5 )
            : base(player, EN_CharacterActionState.EN_CHA_XFP, animName, animLayer)
        {
        }


        protected override void OnEnter( IFsm<Character> fsm )
        {
            if (mCharacter == null) return;
            mCharacter.PlayAnimation(mAnimation, mAnimLayer);
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;
            mCharacter.PlayAnimation("Null", mAnimLayer);
        }
    }
}