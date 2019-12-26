using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateWPipeFire : CharacterState
    {
        private string AttachPoint;

        public CharacterStateWPipeFire( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_WPipeFire, animName, animLayer)
        {
            AttachPoint = "pipepoint";
        }

        protected override void OnInit( IFsm<Character> fsm )
        {
            //mPreviouState = fsm.GetState( (int)EN_CharacterActionState.EN_CHA_WPipeReady );
        }

        //protected override bool CouldEnter( IFsm<FrameworkNetworkPlayer> fsm )
        //{
        //    if (fsm.CurrentState == null || fsm.CurrentState.Id != (int)EN_CharacterActionState.EN_CHA_WPipeReady)
        //    {
        //        return false;
        //    }
        //    InvEquipAttach invEquipAttach = mCharacter._InvEquipAttach;

        //    if (!invEquipAttach || !invEquipAttach.Exsits( AttachPoint ) || invEquipAttach.GetAttachPoint( AttachPoint ).childCount == 0)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        protected override void OnEnter( IFsm<Character> fsm )
        {
//            ScriptEquipAttach scriptEquipAttach = mCharacter.ScriptEquipAttach;
//            string handleName = scriptEquipAttach.GetAttachPoint(AttachPoint).GetChild(0).name;
            //Transform pipe = 
            //Transform pipe = GameObjectRootManager.GetGameObjectRoot<WaterPipeHandle>().Find(handleName);

            //Transform mhq = invEquipAttach.GetAttachPoint( AttachPoint ).GetChild( 0 );

            //Transform point = mhq.gameObject.transform.FindChildNameStartsWith( "Bone018" );
            //ParticleSystem ps = pipe.GetComponentInChildren<ParticleSystem>(true);
            //ps.gameObject.SetActive(true);
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;
            //ScriptEquipAttach scriptEquipAttach = mCharacter.ScriptEquipAttach;
            //if (scriptEquipAttach.GetAttachPoint(AttachPoint).childCount == 0)
            //{
            //    return;
            //}

            //string handleName = scriptEquipAttach.GetAttachPoint(AttachPoint).GetChild(0).name;
            //Transform pipe = 
            //Transform pipe = GameObjectRootManager.GetGameObjectRoot<WaterPipeHandle>().Find(handleName);

            //Transform pipe = invEquipAttach.GetAttachPoint( AttachPoint ).GetChild( 0 );

            //Transform point = mhq.gameObject.transform.FindChildNameStartsWith( "Bone018" );
            //ParticleSystem ps = pipe.GetComponentInChildren<ParticleSystem>(true);
            //ps.gameObject.SetActive(false);


            //base.OnExit( fsm,nextState, isShutdown );
        }

        protected override void OnDestroy( IFsm<Character> fsm )
        {
            base.OnDestroy(fsm);
            AttachPoint = null;
        }
    }
}