public interface IState
{
    bool Enable();

    bool CouldEnter();

    bool CouldLeave();

    void Enter();

    void Leave();
}