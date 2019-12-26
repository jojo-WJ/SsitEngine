using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateWPipeAttach : CharacterState
    {
        public CharacterStateWPipeAttach( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_WPipeAttach, animName, animLayer)
        {
        }
        //public override bool CouldEnter( IFsm<FrameworkNetworkPlayer> fsm )
        //{

        //    //InvEquipAttach invEquipAttach = mCharacter._InvEquipAttach;

        //    //if (!invEquipAttach || !invEquipAttach.Exsits( AttachPoint ) ||
        //    //    invEquipAttach.GetAttachPoint( AttachPoint ).childCount == 0)
        //    //{
        //    //    return false;
        //    //}
        //    return fsm.CurrentState.Id == (int)EN_CharacterActionState.EN_CHA_Stay;

        //    //return true;
        //}

        protected override void OnEnter( IFsm<Character> fsm )
        {
            //mCharacter.thirdPersonManipulator.animator.SetTrigger("PickUp");
            //  vp_Timer.In(0.8f, delegate ()
            //{
            //    mCharacter.State = EN_CharacterActionState.EN_CHA_WPipeReady;
            //});
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
            //pipe.parent = GameObjectRootManager.GetGameObjectRoot<WaterPipeHandle>();

            //RaycastHit hitinfo = new RaycastHit();
            //if (Physics.Raycast(pipe.position + Vector3.down * 0.1f, Vector3.down, out hitinfo, 1000.0f/*, 1 << 8*/))
            //{
            //    pipe.DOMove(hitinfo.point, 0.5f);
            //    pipe.DORotate(new Vector3(90, pipe.localEulerAngles.y, pipe.localEulerAngles.z), 0.5f);

            //}
            //Object.Destroy(scriptEquipAttach.GetAttachPoint(AttachPoint).GetChild(0).gameObject);

            //Debug.LogError("Leave Attach");
            //base.OnExit( fsm,nextState, isShutdown );
        }
    }
}