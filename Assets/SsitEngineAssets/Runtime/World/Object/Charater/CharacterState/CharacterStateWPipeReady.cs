using Framework;
using Framework.SceneObject;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateWPipeReady : CharacterState
    {
        private readonly string animName;
        private string AttachPoint;
        private readonly int layer;

        private readonly Character mPlayer;

        public CharacterStateWPipeReady( Character player, string animName = "hold", int animLayer = 3 )
            : base(player, EN_CharacterActionState.EN_CHA_WPipeReady, animName, animLayer)
        {
            AttachPoint = "pipepoint";
            mPlayer = player;
            this.animName = animName;
            layer = animLayer;
        }

        protected override void OnInit( IFsm<Character> fsm )
        {
            //mPreviouState = fsm.GetState( (int)EN_CharacterActionState.EN_CHA_WPipeAttach );
        }
        //public override bool CouldEnter( IFsm<FrameworkNetworkPlayer> fsm )
        //{
        //    InvEquipAttach invEquipAttach = mCharacter._InvEquipAttach;

        //    if (!invEquipAttach || !invEquipAttach.Exsits( AttachPoint ) ||
        //        invEquipAttach.GetAttachPoint( AttachPoint ).childCount == 0)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        protected override void OnEnter( IFsm<Character> fsm )
        {
            // 切换到拿着水管的动作
            mPlayer.PlayAnimation(animName, layer, false, 0, ( eve, timeElapsed, data ) =>
            {
                //OnEvent(fsm,mCharacter,(ushort)EnCharacterStateEvent.WaterPipeReady,null);
            });
            //mCharacter.thirdPersonManipulator.animator.SetBool("HoldPipe", true);
        }


        protected override void OnExit( IFsm<Character> fsm, bool isShutdown )
        {
            mCharacter?.PlayAnimation("Null", 3);
        }

        protected override void OnDestroy( IFsm<Character> fsm )
        {
            var rope = mCharacter?.GetInteractionObject(InvUseNodeType.InvWaterPipeNode);
            if (rope != null)
                rope.OnChangeProperty(rope, EnPropertyId.OnSwitch, ((int) En_SwitchState.Off).ToString(), mCharacter);
            base.OnDestroy(fsm);
            AttachPoint = null;
        }
    }
}