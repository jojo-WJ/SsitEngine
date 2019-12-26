using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public enum EN_CharacterActionState
    {
        EN_CHA_None = 0,
        EN_CHA_Stay,
        EN_CHA_Assign,

        //EN_CHA_StrecherWait,
        EN_CHA_Strecher,
        EN_CHA_Sentry,

        EN_CHA_DetectorGas,

        /// <summary>
        /// 场景道具的触发状态
        /// </summary>
        EN_CHA_MHQAttach,
        EN_CHA_MHQReady,
        //EN_CHA_MHQFire,

        EN_CHA_Dead,

        EN_CHA_WPipeAttach,
        EN_CHA_WPipeReady,
        EN_CHA_WPipeFire,

        EN_CHA_XFP,

        EN_CHA_Interactoin
    }

    public abstract class CharacterState : FsmState<Character>
    {
        protected string mAnimation;
        protected int mAnimLayer;
        protected Character mCharacter;
        protected EN_CharacterActionState mState;

        public CharacterState( Character player, EN_CharacterActionState state, string animName = "",
            int animLayer = 0 ) : base((int) state)
        {
            mCharacter = player;
            mState = state;
            mAnimation = animName;
            mAnimLayer = animLayer;
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null)
                return;

            //如果当前状态有前置状态需要退出前置状态
            //var prevState = GetPreviousState();
            //if (prevState != null && prevState != nextState && isShutdown)
            //{
            //    this.GetPreviousState().OnExit( fsm, nextState, true );
            //}
        }
    }
}