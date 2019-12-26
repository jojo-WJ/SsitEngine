using Framework.Data;
using UnityEngine;

namespace Framework.SceneObject
{
    public enum TriggleType
    {
        None,
        MouseClick_Triggle,
        MouseClick_OnOff_Triggle,
        MouseClick_Draw_Triggle,

        CharactorDis_Triggle,
    }
    
    public interface ITriggle
    {
        
        TriggleType TriggleType { get; }


        bool Check(BaseSceneInstance baseObj);

        void Enter(BaseSceneInstance sceneObj);

        void Stay(BaseSceneInstance sceneObj);

        void Exit(BaseSceneInstance sceneObj);

        BaseSceneInstance OnPostTriggle(Vector3 point);

    }
}