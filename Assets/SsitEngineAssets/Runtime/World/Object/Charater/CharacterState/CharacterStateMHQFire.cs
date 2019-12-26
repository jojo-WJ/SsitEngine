using Framework.SceneObject;
using SsitEngine.Fsm;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateMHQFire : CharacterState
    {
        public CharacterStateMHQFire( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_MHQAttach /*EN_CharacterActionState.EN_CHA_MHQFire*/,
                animName, animLayer)
        {
        }


        protected override void OnInit( IFsm<Character> fsm )
        {
            //mPreviouState = fsm.GetState((int)EN_CharacterActionState.EN_CHA_MHQReady);
        }


        protected override void OnEnter( IFsm<Character> fsm )
        {
            //ScriptEquipAttach scriptEquipAttach = mCharacter.Equipcon;
            //if (scriptEquipAttach.GetAttachPoint(AttachPoint).childCount == 0)
            //{
            //    return;
            //}
            //Transform mhq = scriptEquipAttach.GetAttachPoint(AttachPoint).GetChild(0);

            //var item = mCharacter.PlayerInfo.mPostObj;
            //if (item != null && item is Annihilator)
            //{
            //    ((Annihilator)item).ApplyItem(true);
            //}
        }

        protected override void OnUpdate( IFsm<Character> fsm, float elapseSeconds )
        {
            //var item = mCharacter.PlayerInfo.mPostObj;
            //if (item != null && item is Annihilator)
            //{
            //    ((Annihilator)item).UseItem();
            //}
        }

        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            if (mCharacter == null) return;
            //ScriptEquipAttach scriptEquipAttach = mCharacter.ScriptEquipAttach;
            //if (scriptEquipAttach.GetAttachPoint(AttachPoint).childCount == 0)
            //{
            //    return;
            //}
            //Transform mhq = scriptEquipAttach.GetAttachPoint(AttachPoint).GetChild(0);

            //Transform point = mhq.gameObject.transform.FindChildNameStartsWith( "Bone018" );
            //var item = mCharacter.PlayerInfo.mPostObj;
            //if (item != null && item is Annihilator)
            //{
            //    ((Annihilator)item).ApplyItem(false);
            //}
            //base.OnExit(fsm, nextState, isShutdown);
        }
    }
}