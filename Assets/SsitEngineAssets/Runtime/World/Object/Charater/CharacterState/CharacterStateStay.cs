using Framework.SceneObject;

namespace SsitEngine.Unity.SceneObject
{
    public class CharacterStateStay : CharacterState
    {
        public CharacterStateStay( Character player, string animName = "", int animLayer = 0 )
            : base(player, EN_CharacterActionState.EN_CHA_Stay, animName, animLayer)
        {
        }
    }
}