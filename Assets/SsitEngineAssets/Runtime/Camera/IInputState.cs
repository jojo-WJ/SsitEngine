using Framework;

public interface IInputState : IState
{
    ENCameraModeType CameraMode { get; }

    void OnUpdate();

    void OnLateUpdate();

    //void OnFixedUpdate();
}