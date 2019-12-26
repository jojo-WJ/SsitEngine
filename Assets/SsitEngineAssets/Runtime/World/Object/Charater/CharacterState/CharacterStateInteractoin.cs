using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateInteractoin : CharacterState
    {
        public CharacterStateInteractoin( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_Interactoin, animName, animLayer)
        {
        }

        protected override void OnEnter( IFsm<Character> fsm )
        {
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
        }
    }
}