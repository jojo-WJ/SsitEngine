namespace Framework.SceneObject
{
    public interface IOnOff
    {
        En_SwitchState GetSwitchState();

        void SetOff(BaseSceneInstance triggler);

        void SetOn(BaseSceneInstance triggler);

        void SetOn(bool isOn,BaseSceneInstance triggler);

        string GetTriggler();

        bool CanDisplay();

        bool InInterationArea();
    }
}