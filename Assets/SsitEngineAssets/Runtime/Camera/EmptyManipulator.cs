/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/11 18:54:09                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;

public class EmptyManipulator : IInputState
{
    public ENCameraModeType CameraMode => ENCameraModeType.EmptyManipulator;

    public bool Enable()
    {
        return true;
    }

    public bool CouldEnter()
    {
        return true;
    }

    public bool CouldLeave()
    {
        return true;
    }

    public void Enter()
    {
    }

    public void Leave()
    {
    }

    public void OnUpdate()
    {
    }

    public void OnLateUpdate()
    {
    }

    public void OnFixedUpdate()
    {
    }
}