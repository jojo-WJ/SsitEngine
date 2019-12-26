using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateStrecherWait : CharacterState
    {
        public CharacterStateStrecherWait( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_None /*EN_CharacterActionState.EN_CHA_StrecherWait*/,
                animName, animLayer)
        {
        }

        protected override void OnEnter( IFsm<Character> fsm )
        {
            //if (mCharacter.OnHUDChange != null)
            //{
            //    mCharacter.OnHUDStateChange.Invoke("等待协助中......");
            //}
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;
            //if (mCharacter.OnHUDChange != null)
            //{
            //    mCharacter.OnHUDStateChange.Invoke("");
            //}
        }
    }
}